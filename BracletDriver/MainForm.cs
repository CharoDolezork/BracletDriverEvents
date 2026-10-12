namespace BracletDriver;
internal partial class MainForm : Form
{
    private readonly Options _opt;
    private readonly ProgressLog _log;
    private readonly Camera _camera;
    private readonly RodPort _port;
    private readonly SerialRodDriver _driver;
    private readonly FrameAnalyzer _analyzer;
    private readonly Guidance _guidance;
    private readonly PreviewRenderer _preview;

    private bool _driveBusy;

    // Ручной подбор выдержки для отверстий
    private bool _holeTuning;
    private bool _analyzerWasEnabled;
    private int _exposureBefore;

    private int _roiClick;
    private Point _roiCorner;

    public MainForm()
    {
        InitializeComponent();

        _log = new ProgressLog(txtLog);
        _opt = Options.Load(out string? error);
        if (error is not null) _log.Log(error);

        _camera = new Camera(_log);
        _port = new RodPort(_log);
        _driver = new SerialRodDriver(_port, _opt, _log);
        _analyzer = new FrameAnalyzer(_log, _opt, _camera);
        _guidance = new Guidance(_opt, _log, _camera, _analyzer, _driver);
        _preview = new PreviewRenderer(_opt, _log, _camera, _analyzer);
        _preview.PreviewReady += OnPreviewReady;

        UpdateUi();
        timerUi.Start();
    }

    #region Подключение
    private void btnPort_Click(object sender, EventArgs e)
    {
        if (_port.IsOpen)
        {
            DisableAnalyzer();
            _port.Close();
        }
        else
        {
            _port.Open(_opt);
        }

        UpdateUi();
    }

    private void btnCamera_Click(object sender, EventArgs e)
    {
        if (_camera.IsOpen)
        {
            DisableAnalyzer();
            _camera.Close();
            SetPreview(null);
        }
        else
        {
            _camera.Open(_opt);
        }

        UpdateUi();
    }

    private void btnAnalyzer_Click(object sender, EventArgs e)
    {
        if (_analyzer.Enabled)
        {
            DisableAnalyzer();
        }
        else
        {
            _analyzer.Enabled = true;
            _guidance.Start(Guidance.Stage.Exposure, out _);
        }

        UpdateUi();
    }

    private void DisableAnalyzer()
    {
        if (!_analyzer.Enabled) return;

        _guidance.Stop();
        _analyzer.Enabled = false;
        _camera.SetExposure(_opt.CameraExposureStart);
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using var form = new SettingsForm(_opt);
        if (form.ShowDialog(this) != DialogResult.OK) return;

        SaveOptions();
    }

    private void SaveOptions()
    {
        if (!_opt.Save(out string? error)) _log.Log(error!);
    }
    #endregion

    #region Превью
    private void chkLayer_CheckedChanged(object sender, EventArgs e)
    {
        var layers = PreviewLayers.None;
        if (chkRoi.Checked) layers |= PreviewLayers.Roi;
        if (chkTarget.Checked) layers |= PreviewLayers.Target;
        if (chkProbe.Checked) layers |= PreviewLayers.Probe;
        if (chkRodTip.Checked) layers |= PreviewLayers.RodTip;
        if (chkHoles.Checked) layers |= PreviewLayers.Holes;

        _preview.Layers = layers;
    }

    private void OnPreviewReady(Bitmap bitmap)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            bitmap.Dispose();
            return;
        }

        try
        {
            BeginInvoke(new Action(() => SetPreview(bitmap)));
        }
        catch (InvalidOperationException)
        {
            bitmap.Dispose();
        }
    }

    private void SetPreview(Image? image)
    {
        var old = picPreview.Image;
        picPreview.Image = image;
        old?.Dispose();
    }

    private void btnPickRoi_Click(object sender, EventArgs e)
    {
        if (_roiClick != 0) CancelRoiPick();
        else SetRoiClick(1);
    }

    private void picPreview_MouseClick(object sender, MouseEventArgs e)
    {
        if (_roiClick == 0 || ToFramePoint(e.Location) is not { } point) return;

        if (_roiClick == 1)
        {
            _roiCorner = point;
            SetRoiClick(2);
            return;
        }

        _opt.RoiX1 = _roiCorner.X;
        _opt.RoiY1 = _roiCorner.Y;
        _opt.RoiX2 = point.X;
        _opt.RoiY2 = point.Y;
        SaveOptions();

        _log.Log($"Рамка: ({_opt.RoiX1}, {_opt.RoiY1}) — ({_opt.RoiX2}, {_opt.RoiY2}).");
        SetRoiClick(0);
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape && _roiClick != 0) CancelRoiPick();
    }

    private void CancelRoiPick() => SetRoiClick(0);

    private void SetRoiClick(int click)
    {
        _roiClick = click;
        lblPickRoi.Text = click switch
        {
            1 => "Кликните первый угол рамки (Esc — отмена)",
            2 => "Кликните второй угол рамки (Esc — отмена)",
            _ => "",
        };
        UpdateUi();
    }

    // Клик по PictureBox в режиме Zoom → пиксель кадра; null — мимо картинки
    private Point? ToFramePoint(Point click)
    {
        if (picPreview.Image is not { } image) return null;

        double scale = Math.Min((double)picPreview.Width / image.Width, (double)picPreview.Height / image.Height);
        double left = (picPreview.Width - image.Width * scale) / 2;
        double top = (picPreview.Height - image.Height * scale) / 2;

        int x = (int)Math.Round((click.X - left) / scale);
        int y = (int)Math.Round((click.Y - top) / scale);
        if (x < 0 || y < 0 || x > image.Width || y > image.Height) return null;

        return new Point(x, y);
    }
    #endregion

    #region Наведение
    private void btnStart_Click(object sender, EventArgs e) => StartGuidance(Guidance.Stage.Descent);

    private void btnFindTarget_Click(object sender, EventArgs e) => StartGuidance(Guidance.Stage.Target);

    private void btnTuneExposure_Click(object sender, EventArgs e) =>
        StartGuidance(Guidance.Stage.Exposure, rememberRodExposure: true);

    private void btnCalibrate_Click(object sender, EventArgs e) => StartGuidance(Guidance.Stage.Calibration);

    private void StartGuidance(Guidance.Stage stopAfter, bool rememberRodExposure = false)
    {
        if (!_guidance.Start(stopAfter, out string? error, rememberRodExposure) && error is not null)
            MessageBox.Show(this, error, "Калибровка", MessageBoxButtons.OK, MessageBoxIcon.Error);

        UpdateUi();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _guidance.Stop();
        UpdateUi();
    }
    #endregion

    #region Выдержка для отверстий
    private void btnHoleExposure_Click(object sender, EventArgs e)
    {
        if (_holeTuning) EndHoleTuning(save: true);
        else BeginHoleTuning();

        UpdateUi();
    }

    private void btnHoleCancel_Click(object sender, EventArgs e)
    {
        EndHoleTuning(save: false);
        UpdateUi();
    }

    // Непрерывный поиск отверстий: оператор сам меняет выдержку, пока результат его не устроит
    private void BeginHoleTuning()
    {
        _analyzerWasEnabled = _analyzer.Enabled;
        _exposureBefore = _camera.CurrentExposure;

        _analyzer.Enabled = true;
        _analyzer.HoleSearchEnabled = true;
        _analyzer.RodSearchEnabled = false;
        _camera.SetExposure(_opt.HoleExposure);

        _holeTuning = true;
    }

    private void EndHoleTuning(bool save)
    {
        if (save)
        {
            _opt.HoleExposure = _camera.CurrentExposure;
            _log.Log($"Выдержка для отверстий запомнена: {_opt.HoleExposure}.");
            SaveOptions();
        }

        _holeTuning = false;

        if (_analyzerWasEnabled)
        {
            _analyzer.ResetSearch();
            _camera.SetExposure(_exposureBefore);
        }
        else
        {
            DisableAnalyzer();
        }
    }

    private void btnExposureDown_Click(object sender, EventArgs e) => AdjustExposure(-1);

    private void btnExposureUp_Click(object sender, EventArgs e) => AdjustExposure(1);

    private void AdjustExposure(int delta) =>
        _camera.SetExposure(Math.Clamp(_camera.CurrentExposure + delta, _opt.CameraExposureMin, 0));
    #endregion

    #region Привод
    private void btnBaseline_Click(object sender, EventArgs e) => MoveTo(_opt.RodBaselinePosition);

    private void btnRoiPosition_Click(object sender, EventArgs e) => MoveTo(_opt.RodRoiPosition);

    private void btnMiddle_Click(object sender, EventArgs e) => MoveTo(_opt.RodMiddlePosition);

    private void btnHome_Click(object sender, EventArgs e) => MoveTo(_opt.RodHomePosition);

    private void btnDriveStop_Click(object sender, EventArgs e) => _driver.Stop();

    private void btnMotorsOn_Click(object sender, EventArgs e) => _driver.EnableMotors();

    private void btnMotorsOff_Click(object sender, EventArgs e) => _driver.DisableMotors();

    // Ход блокирует поток до ответа платы, поэтому идёт в фоне
    private async void MoveTo(int[] position)
    {
        _driveBusy = true;
        UpdateUi();

        try
        {
            await Task.Run(() =>
            {
                if (_driver.TryStart()) _driver.GoTo(position);
            });
        }
        catch (Exception ex)
        {
            _log.Log($"Ошибка хода: {ex.Message}");
        }
        finally
        {
            _driveBusy = false;
            UpdateUi();
        }
    }

    private void btnSend_Click(object sender, EventArgs e) => SendCommand();

    private void txtCommand_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;

        e.SuppressKeyPress = true;
        SendCommand();
    }

    private void SendCommand()
    {
        string command = txtCommand.Text.Trim();
        if (command.Length == 0 || !_port.IsOpen) return;

        _port.Send(command);
        txtCommand.Clear();
    }
    #endregion

    #region Состояние
    private void timerUi_Tick(object sender, EventArgs e) => UpdateUi();

    private void UpdateUi()
    {
        bool port = _port.IsOpen;
        bool camera = _camera.IsOpen;
        bool analyzer = _analyzer.Enabled;
        bool running = _guidance.Running;
        bool idle = !running && _roiClick == 0 && !_holeTuning;

        btnPort.Text = port ? "Закрыть порт" : "Открыть порт";
        btnPort.Enabled = idle;

        btnCamera.Text = camera ? "Закрыть камеру" : "Открыть камеру";
        btnCamera.Enabled = idle;

        btnAnalyzer.Text = analyzer ? "Выключить анализатор" : "Включить анализатор";
        btnAnalyzer.Enabled = idle && (analyzer || port && camera);

        btnSettings.Enabled = idle;
        btnPickRoi.Enabled = camera && !running && !_holeTuning;

        btnHoleExposure.Text = _holeTuning ? "Запомнить выдержку" : "Подобрать выдержку для отверстий";
        btnHoleExposure.Enabled = camera && !running && _roiClick == 0;
        btnHoleCancel.Enabled = _holeTuning;
        btnExposureDown.Enabled = _holeTuning;
        btnExposureUp.Enabled = _holeTuning;
        lblHole.Text = _holeTuning && _analyzer.Holes is { } holes
            ? $"найдено {holes.Marks.Length} отверстий в {holes.ChainCount} цепочках"
            : "";

        foreach (var button in new[] { btnStart, btnFindTarget, btnTuneExposure, btnCalibrate })
            button.Enabled = idle && analyzer && port;
        btnStop.Enabled = running;

        foreach (var button in new[] { btnBaseline, btnRoiPosition, btnMiddle, btnHome })
            button.Enabled = idle && port && !_driveBusy;
        btnDriveStop.Enabled = port;
        btnMotorsOn.Enabled = idle && port && !_driveBusy;
        btnMotorsOff.Enabled = idle && port && !_driveBusy;

        txtCommand.Enabled = idle && port;
        btnSend.Enabled = idle && port;

        lblStatus.Text = $"порт: {(port ? "открыт" : "закрыт")} · камера: {(camera ? "открыта" : "закрыта")} · " +
                         $"анализатор: {(analyzer ? "включён" : "выключен")}";
        lblGuidance.Text = $"этап: {_guidance.CurrentStage}";
        lblExposure.Text = $"выдержка: {_camera.CurrentExposure}";
        lblPosition.Text = $"позиция: {string.Join(" ", _driver.Position)}";
    }
    #endregion

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        timerUi.Stop();
        _preview.PreviewReady -= OnPreviewReady;

        _guidance.Stop();
        _analyzer.Enabled = false;

        _preview.Dispose();
        _guidance.Dispose();
        _camera.Dispose();
        _analyzer.Dispose();
        _port.Dispose();

        SetPreview(null);
    }
}
