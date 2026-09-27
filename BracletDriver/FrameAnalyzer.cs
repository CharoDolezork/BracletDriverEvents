using OpenCvSharp;

namespace BracletDriver;


internal class FrameAnalyzer : IDisposable
{
    private readonly ProgressLog _log;
    private readonly Options _opt;
    private readonly Camera _camera;
    private readonly RodDetector _detector;

    private bool _enabled;
    public bool Enabled
    {
        get => Volatile.Read(ref _enabled);
        set => Volatile.Write(ref _enabled, value);
    }

    private bool _holeSearchEnabled;
    public bool HoleSearchEnabled
    {
        get => Volatile.Read(ref _holeSearchEnabled);
        set => Volatile.Write(ref _holeSearchEnabled, value);
    }

    private bool _rodSearchEnabled;
    public bool RodSearchEnabled
    {
        get => Volatile.Read(ref _rodSearchEnabled);
        set => Volatile.Write(ref _rodSearchEnabled, value);
    }

    private int _busy;
    public bool Busy
    {
        get => Volatile.Read(ref _busy) != 0;
        private set => Volatile.Write(ref _busy, value ? 1 : 0);
    }

    public event Action<FrameAnalysis>? FrameAnalyzed;

    public FrameAnalyzer(ProgressLog log, Options opt, Camera camera)
    {
        _log = log;
        _opt = opt;
        _camera = camera;
        _detector = new RodDetector(opt, log);
        _camera.FrameReceived += OnFrameReceived;
    }


    private void OnFrameReceived(CameraFrame frame)
    {
        if (!Enabled) return;
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return;

        var copy = new CameraFrame(frame.Frame.Clone(), frame.FrameNumber, frame.Timestamp);

        Task.Run(() => Analyze(copy));
    }

    private Point2d? FindHole(Mat frame)
    {
        using var grayFrame = new Mat();
        Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);

        var cands = CandidateDetector.Detect(grayFrame, _opt);
        var chains = ChainFinder.Find(cands, _opt);
        var target = ChainFinder.SelectTarget(chains);

        return target?.Center;
    }

    private void Analyze(CameraFrame frame)
    {
        try
        {
            var hole = HoleSearchEnabled ? FindHole(frame.Frame) : null;
            var (rodTip, brightShare) = _detector.Detect(frame.Frame, RodSearchEnabled);

            FrameAnalyzed?.Invoke(new FrameAnalysis(frame, hole, rodTip, brightShare));
        }
        catch (Exception ex)
        {
            _log.Log($"Ошибка анализа кадра {frame.FrameNumber}: {ex.Message}");
        }
        finally
        {
            frame.Dispose();
            Volatile.Write(ref _busy, 0);
        }
    }

    public void Dispose()
    {
        _camera.FrameReceived -= OnFrameReceived;
        Enabled = false;

        long deadline = Clock.After(2000);
        while (Volatile.Read(ref _busy) != 0)
        {
            if (Clock.Passed(deadline))
            {
                _log.Log("Анализ не завершился за 2 секунды — детектор не освобождён.");
                return;
            }

            Thread.Sleep(10);
        }

        _detector.Dispose();
    }
}

public sealed record FrameAnalysis(CameraFrame Frame, Point2d? Hole, Point2d? RodTip, double BrightShare);
