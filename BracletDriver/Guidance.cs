using OpenCvSharp;

namespace BracletDriver;
internal sealed class Guidance : IDisposable
{
    public enum Stage
    {
        Idle,
        Baseline,
        Target,
        Exposure,
        Calibration,
        Approach,
        Descent,
    }

    private readonly Options _opt;
    private readonly ProgressLog _log;
    private readonly Camera _camera;
    private readonly FrameAnalyzer _analyzer;
    private readonly SerialRodDriver _driver;
    private readonly MotionModel _model;

    private bool _running;
    public bool Running
    {
        get => Volatile.Read(ref _running);
        private set => Volatile.Write(ref _running, value);
    }

    private int _busy;

    private long _readyAt;

    private Stage _stopAfter;
    private bool _rememberRodExposure;
    private bool _useSavedCalibration;

    private int _lostFrames;
    private int _steps;
    private Point2d _target;
    private double _probeReference;

    private int _calibMotor;
    private bool _calibProbed;
    private Point2d _calibBase;

    private Point2d? _tipBeforeMove;
    private double[]? _appliedMove;

    public Guidance(Options opt, ProgressLog log, Camera camera, FrameAnalyzer analyzer, SerialRodDriver driver)
    {
        _opt = opt;
        _log = log;
        _camera = camera;
        _analyzer = analyzer;
        _driver = driver;
        _model = new MotionModel(opt);

        _analyzer.FrameAnalyzed += OnFrameAnalyzed;
        _driver.MoveDone += OnMoveDone;
    }

    private Stage _stage = Stage.Idle;
    private long _stageStartedAt;

    public Stage CurrentStage
    {
        get => _stage;
        private set
        {
            if (value == _stage) return;

            if (_stage != Stage.Idle)
                _log.Log($"Стадия {_stage}: {Clock.Since(_stageStartedAt) / 1000.0:F1} с.");

            _stage = value;
            _stageStartedAt = Clock.Ms;
        }
    }


    public bool Start(Stage stopAfter, out string? error, bool rememberRodExposure = false)
    {
        error = null;
        if (Running) return false;

        // Подход и спуск идут по таблице из файла, пробные ходы делает только прогон «до калибровки»
        _useSavedCalibration = stopAfter > Stage.Calibration;
        if (_useSavedCalibration && !_model.TryLoad(Options.CalibrationPath, out error)) return false;

        _stopAfter = stopAfter;
        _rememberRodExposure = rememberRodExposure;
        _readyAt = 0;
        _lostFrames = 0;
        _steps = 0;
        _tipBeforeMove = null;
        _appliedMove = null;
        CurrentStage = Stage.Baseline;

        Running = true;

        _log.Log($"Наведение: старт (до стадии {stopAfter}).");
        return true;
    }

    public void Stop()
    {
        if (!Running) return;

        Running = false;
        _driver.Stop();
        Release();

        _log.Log("Наведение остановлено.");
    }

    private void Finish(string reason)
    {
        Running = false;
        Release();

        _log.Log($"Наведение завершено: {reason}");
    }

    private void Release()
    {
        CurrentStage = Stage.Idle;
        _analyzer.ResetSearch();
    }

    // Прогон до этой стадии закончен — останавливаемся
    private bool StopsAfter(Stage stage)
    {
        if (_stopAfter != stage) return false;

        Finish($"стадия {stage} пройдена.");
        return true;
    }

    private void OnMoveDone(bool success)
    {
        if (!Running) return;

        if (!success)
        {
            Finish("плата не подтвердила окончание хода.");
            return;
        }

        Volatile.Write(ref _readyAt, Clock.After(_opt.GuidanceSettleMs));
    }

    private void OnFrameAnalyzed(FrameAnalysis frame)
    {
        if (!Running) return;
        if (frame.Timestamp < Volatile.Read(ref _readyAt)) return;
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return;

        Task.Run(() =>
        {
            try { Process(frame); }
            catch (Exception ex) { Finish($"ошибка: {ex.Message}"); }
            finally { Volatile.Write(ref _busy, 0); }
        });
    }

    private void Process(FrameAnalysis frame)
    {
        switch (CurrentStage)
        {
            case Stage.Baseline: GoToBaseline(); break;
            case Stage.Target: SelectTarget(frame.Hole); break;
            case Stage.Exposure: TuneExposure(frame.Exposure, frame.MaxBrightness, frame.ProbeBrightness); break;
            case Stage.Calibration: Calibrate(frame.RodTip, frame.ProbeBrightness); break;
            case Stage.Approach:
            case Stage.Descent: Guide(frame.RodTip, frame.ProbeBrightness); break;
        }
    }

    private double SlowdownFactor(double delta)
    {
        double t = (delta - _opt.GuidanceSlowdownStart) / (_opt.GuidanceSlowdownFull - _opt.GuidanceSlowdownStart);
        return 1 - Math.Clamp(t, 0, 1) * (1 - _opt.GuidanceMinStepFraction);
    }

    private void GoToBaseline()
    {
        if (!_driver.TryStart())
        {
            Finish("плата не сообщила позиции моторов.");
            return;
        }

        _analyzer.HoleSearchEnabled = true;
        _analyzer.RodSearchEnabled = false;
        _camera.SetExposure(_opt.HoleExposure);

        _log.Log("Наведение: перевод в эталонную позицию.");

        if (!Running) return;
        _driver.GoTo(_opt.RodBaselinePosition);

        if (Running) CurrentStage = Stage.Target;
    }

    private void SelectTarget(Point2d? hole)
    {
        if (hole is not { } target)
        {
            if (++_lostFrames > _opt.GuidanceMaxLostFrames) Finish("на эталоне не найдено отверстие.");
            return;
        }

        _target = target;
        _analyzer.Target = target;
        _lostFrames = 0;
        _analyzer.HoleSearchEnabled = false;

        _log.Log($"Цель выбрана: ({_target.X:F1}, {_target.Y:F1}).");
        if (StopsAfter(Stage.Target)) return;

 
        int rodExposure = _rememberRodExposure ? _opt.CameraExposureStart : _opt.RodExposure;
        if (_camera.CurrentExposure != rodExposure)
        {
            _camera.SetExposure(rodExposure);
            Volatile.Write(ref _readyAt, Clock.After(_opt.GuidanceSettleMs));
        }

        _log.Log("Подбираю выдержку.");
        CurrentStage = Stage.Exposure;
    }

    private void TuneExposure(int exposure, double maxBrightness, double probe)
    {
        // Кадр снят до смены выдержки — по нему решать нельзя
        if (exposure != _camera.CurrentExposure) return;

        if (maxBrightness >= _opt.RodBrightThreshold)
        {
            int next = _camera.CurrentExposure - 1;
            if (next < _opt.CameraExposureMin)
            {
                Finish($"выдержка на пределе ({_camera.CurrentExposure}), " +
                       $"а самый яркий пиксель всё ещё {maxBrightness:F0}.");
                return;
            }

            _camera.SetExposure(next);
            return;
        }

        if (double.IsNaN(probe)) return;

        _probeReference = probe;
        _log.Log($"Выдержка подобрана ({_camera.CurrentExposure}), самый яркий пиксель {maxBrightness:F0}, " +
                 $"окно яркости {probe:F0}.");

        if (_rememberRodExposure)
        {
            _opt.RodExposure = _camera.CurrentExposure;
            _log.Log($"Выдержка для стержня запомнена: {_opt.RodExposure}.");
            if (!_opt.Save(out string? error)) _log.Log(error!);
        }

        if (StopsAfter(Stage.Exposure)) return;

        _log.Log("Выдвигаю стержень.");

        _analyzer.RodSearchEnabled = true;
        // С сохранённой таблицей пробные ходы не нужны: стержень только выезжает в «Рамку»
        _calibMotor = _useSavedCalibration ? _driver.MotorCount : 0;
        _calibProbed = false;
        CurrentStage = Stage.Calibration;

        if (!Running) return;
        _driver.GoTo(_opt.RodRoiPosition);
    }

    private void Calibrate(Point2d? rodTip, double probeBrightness)
    {
        if (rodTip is not { } tip)
        {
            if (++_lostFrames > _opt.GuidanceMaxLostFrames) Finish("кончик стержня потерян.");
            return;
        }
        _lostFrames = 0;

        double probe = _opt.GuidanceCalibrationStep;

        if (_calibProbed)
        {
            var shift = tip - _calibBase;
            _model.SetColumn(_calibMotor, shift, probe);
            _log.Log($"Калибровка, мотор {_calibMotor + 1}: сдвиг ({shift.X:F1}, {shift.Y:F1}) px.");

            _calibProbed = false;

            if (!Running) return;
            _driver.Step(Probe(_calibMotor++, -probe));
            return;
        }

        if (_calibMotor == _driver.MotorCount)
        {
            if (_useSavedCalibration)
            {
                _log.Log($"Калибровка из файла. Таблица: {_model}.");
            }
            else
            {
                _log.Log($"Калибровка закончена. Таблица: {_model}.");
                SaveCalibration();
            }

            if (StopsAfter(Stage.Calibration)) return;

            CurrentStage = Stage.Approach;
            Guide(rodTip, probeBrightness);
            return;
        }

        _calibBase = tip;
        _calibProbed = true;

        if (!Running) return;
        _driver.Step(Probe(_calibMotor, probe));
    }

    private void SaveCalibration()
    {
        try
        {
            _model.Save(Options.CalibrationPath);
            _log.Log($"Калибровка сохранена: {Options.CalibrationPath}");
        }
        catch (Exception ex)
        {
            _log.Log($"Не удалось сохранить калибровку: {ex.Message}");
        }
    }

    private double[] Probe(int motor, double step)
    {
        var vector = new double[_driver.MotorCount];
        vector[motor] = step;
        return vector;
    }

    private void Guide(Point2d? rodTip, double probe)
    {
        if (rodTip is not { } tip)
        {
            if (++_lostFrames > _opt.GuidanceMaxLostFrames) Finish("кончик стержня потерян.");
            return;
        }
        _lostFrames = 0;

        if (_tipBeforeMove is { } before && _appliedMove is { } applied)
        {
            bool updated = _model.Update(applied, tip - before, out double residual);
            _log.Log(updated
                ? $"Таблица уточнена (ошибка {residual:F1} px)."
                : $"Таблица не правится этим ходом (ошибка {residual:F1} px).");
            _tipBeforeMove = null;
            _appliedMove = null;
        }

        if (_steps >= _opt.GuidanceMaxSteps)
        {
            Finish($"сделано {_steps} шагов — лимит.");
            return;
        }

        var error = _target - tip;
        double miss = Math.Sqrt(error.X * error.X + error.Y * error.Y);

        double[]? step;
        if (miss > _opt.GuidanceOnRayRadius)
        {
            step = _model.StepTo(error, _opt.GuidanceMaxStep);
        }
        else
        {
            if (CurrentStage == Stage.Approach)
            {
                CurrentStage = Stage.Descent;
                _log.Log("Стержень на луче — спуск.");
            }
            step = _model.DescentStep(_opt.GuidanceDescentStep);
        }

        if (step is null)
        {
            Finish("таблица вырождена, шаг не посчитать.");
            return;
        }

        double delta = probe - _probeReference;
        double factor = SlowdownFactor(delta);
        for (int i = 0; i < step.Length; i++) step[i] *= factor;

        _steps++;
        _log.Log($"Шаг {_steps} ({(miss > _opt.GuidanceOnRayRadius ? "к лучу" : "спуск")}): " +
                 $"промах {miss:F1} px, окно Δ{delta:F0} → шаг ×{factor:F2}, " +
                 $"ход [{string.Join(", ", step.Select(s => s.ToString("F0")))}].");

        if (!Running) return;

        _tipBeforeMove = tip;
        _driver.Step(step);
        _appliedMove = (double[])_driver.Applied.Clone();
    }

    public void Dispose()
    {
        _analyzer.FrameAnalyzed -= OnFrameAnalyzed;
        _driver.MoveDone -= OnMoveDone;
    }
}
