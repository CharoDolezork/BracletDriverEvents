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

    private int _lostFrames;
    private int _steps;
    private Point2d _target;

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

    public Stage CurrentStage { get; private set; } = Stage.Idle;

    public void Start()
    {
        if (Running) return;

        _readyAt = 0;
        _lostFrames = 0;
        _steps = 0;
        _tipBeforeMove = null;
        _appliedMove = null;
        CurrentStage = Stage.Baseline;

        _analyzer.Enabled = true;
        Running = true;

        _log.Log("Наведение: старт.");
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
        _analyzer.Enabled = false;
        _analyzer.HoleSearchEnabled = true;
        _analyzer.RodSearchEnabled = false;
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
        if (frame.Frame.Timestamp < Volatile.Read(ref _readyAt)) return;
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return;

        var hole = frame.Hole;
        var rodTip = frame.RodTip;
        double brightShare = frame.BrightShare;

        Task.Run(() =>
        {
            try { Process(hole, rodTip, brightShare); }
            catch (Exception ex) { Finish($"ошибка: {ex.Message}"); }
            finally { Volatile.Write(ref _busy, 0); }
        });
    }

    private void Process(Point2d? hole, Point2d? rodTip, double brightShare)
    {
        switch (CurrentStage)
        {
            case Stage.Baseline: GoToBaseline(); break;
            case Stage.Target: SelectTarget(hole); break;
            case Stage.Exposure: TuneExposure(brightShare); break;
            case Stage.Calibration: Calibrate(rodTip); break;
            case Stage.Approach:
            case Stage.Descent: Guide(rodTip); break;
        }
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
        _camera.SetExposure(_opt.CameraExposureStart);

        _log.Log("Наведение: перевод в эталонную позицию.");
        CurrentStage = Stage.Target;

        if (!Running) return;
        _driver.GoTo(_opt.RodBaselinePosition);
    }

    private void SelectTarget(Point2d? hole)
    {
        if (hole is not { } target)
        {
            if (++_lostFrames > _opt.GuidanceMaxLostFrames) Finish("на эталоне не найдено отверстие.");
            return;
        }

        _target = target;
        _lostFrames = 0;
        _analyzer.HoleSearchEnabled = false;

        _log.Log($"Цель выбрана: ({_target.X:F1}, {_target.Y:F1}). Подбираю выдержку.");
        CurrentStage = Stage.Exposure;
    }

    private void TuneExposure(double brightShare)
    {
        if (brightShare > _opt.ExposureBrightShareLimit)
        {
            int next = _camera.CurrentExposure - 1;
            if (next < _opt.CameraExposureMin)
            {
                Finish($"выдержка на пределе ({_camera.CurrentExposure}), " +
                       $"а ярких пикселей всё ещё {brightShare:P2}.");
                return;
            }

            _camera.SetExposure(next);
            return;
        }

        _log.Log($"Выдержка подобрана ({_camera.CurrentExposure}), ярких пикселей {brightShare:P2}. " +
                 "Выдвигаю стержень.");

        _analyzer.RodSearchEnabled = true;
        _calibMotor = 0;
        _calibProbed = false;
        CurrentStage = Stage.Calibration;

        if (!Running) return;
        _driver.GoTo(_opt.RodRoiPosition);
    }

    private void Calibrate(Point2d? rodTip)
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
            _log.Log($"Калибровка закончена. Таблица: {_model}.");
            CurrentStage = Stage.Approach;
            Guide(rodTip);
            return;
        }

        _calibBase = tip;
        _calibProbed = true;

        if (!Running) return;
        _driver.Step(Probe(_calibMotor, probe));
    }

    private double[] Probe(int motor, double step)
    {
        var vector = new double[_driver.MotorCount];
        vector[motor] = step;
        return vector;
    }

    private void Guide(Point2d? rodTip)
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

        _steps++;
        _log.Log($"Шаг {_steps} ({(miss > _opt.GuidanceOnRayRadius ? "к лучу" : "спуск")}): " +
                 $"промах {miss:F1} px, ход [{string.Join(", ", step.Select(s => s.ToString("F0")))}].");

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
