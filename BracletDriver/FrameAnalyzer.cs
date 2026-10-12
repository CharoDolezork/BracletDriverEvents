using OpenCvSharp;

namespace BracletDriver;
internal sealed class FrameAnalyzer : IDisposable
{
    private readonly ProgressLog _log;
    private readonly Options _opt;
    private readonly Camera _camera;
    private readonly RodDetector _detector;

    private bool _enabled;
    public bool Enabled
    {
        get => Volatile.Read(ref _enabled);
        set
        {
            Volatile.Write(ref _enabled, value);
            if (value) return;

            Target = null;
            Volatile.Write(ref _holes, null);
            ResetSearch();
        }
    }

    private bool _holeSearchEnabled;
    public bool HoleSearchEnabled
    {
        get => Volatile.Read(ref _holeSearchEnabled);
        set => Volatile.Write(ref _holeSearchEnabled, value);
    }

    private bool _rodSearchEnabled = true;
    public bool RodSearchEnabled
    {
        get => Volatile.Read(ref _rodSearchEnabled);
        set => Volatile.Write(ref _rodSearchEnabled, value);
    }

    private TargetWindow? _target;
    public Point2d? Target
    {
        get => Volatile.Read(ref _target)?.Point;
        set => Volatile.Write(ref _target, value is { } point ? new TargetWindow(point, ProbeWindow(point)) : null);
    }

    private HoleSet? _holes;
    public HoleSet? Holes => Volatile.Read(ref _holes);

    private int _busy;
    public bool Busy
    {
        get => Volatile.Read(ref _busy) != 0;
        private set => Volatile.Write(ref _busy, value ? 1 : 0);
    }

    public event Action<FrameAnalysis>? FrameAnalyzed;
    public event Action<PreviewFrame>? PreviewRequested;

    public FrameAnalyzer(ProgressLog log, Options opt, Camera camera)
    {
        _log = log;
        _opt = opt;
        _camera = camera;
        _detector = new RodDetector(opt);
        _camera.FrameReceived += OnFrameReceived;
    }

    public void ResetSearch()
    {
        HoleSearchEnabled = false;
        RodSearchEnabled = true;
    }

    private void OnFrameReceived(CameraFrame frame)
    {
        if (!Enabled) return;
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return;

        var copy = frame with { Frame = frame.Frame.Clone() };

        Task.Run(() => Analyze(copy));
    }

    private (Point2d? Hole, HoleSet Holes) FindHole(Mat frame, Point2d offset)
    {
        using var grayFrame = new Mat();
        Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);

        var cands = AdaptiveCandidateDetector.Detect(grayFrame, _opt);
        var chains = ChainFinder.Find(cands, _opt);
        var target = ChainFinder.SelectTarget(chains);

        var marks = chains
            .SelectMany(chain => chain.Points)
            .Select(point => new HoleMark(point.Center + offset, point.Radius))
            .ToArray();

        return (target?.Center + offset, new HoleSet(marks, chains.Count));
    }

    private Rect ProbeWindow(Point2d target)
    {
        int d = _opt.GuidanceProbeOffset;
        var (dx, dy) = _opt.RodTipSide switch
        {
            Side.Up => (0, -d),
            Side.Down => (0, d),
            Side.Left => (-d, 0),
            _ => (d, 0),
        };
        return new Rect((int)Math.Round(target.X) + dx - 1, (int)Math.Round(target.Y) + dy - 1, 3, 3);
    }

    private double ProbeBrightness(Mat frame, Rect window)
    {
        window &= new Rect(0, 0, frame.Width, frame.Height);
        if (window.Width == 0 || window.Height == 0) return double.NaN;

        using var view = new Mat(frame, window);
        return Cv2.Mean(view)[_opt.RodChannel];
    }

    private void Analyze(CameraFrame frame)
    {
        try
        {
            var roi = Roi.Resolve(_opt.Roi, frame.Frame.Size());
            var offset = new Point2d(roi.X, roi.Y);
            using var view = new Mat(frame.Frame, roi);

            Point2d? hole = null;
            if (HoleSearchEnabled)
            {
                var search = FindHole(view, offset);
                hole = search.Hole;
                Volatile.Write(ref _holes, search.Holes);
            }

            var (rodTip, maxBrightness) = _detector.Detect(view, RodSearchEnabled);
            rodTip += offset;

            var target = Volatile.Read(ref _target);
            double probe = target is null ? double.NaN : ProbeBrightness(frame.Frame, target.Window);

            FrameAnalyzed?.Invoke(new FrameAnalysis(frame.Timestamp, frame.Exposure, hole, rodTip, maxBrightness, probe));
            PreviewRequested?.Invoke(new PreviewFrame(frame.Frame, roi, Holes, rodTip, target?.Point, target?.Window));
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

    private sealed record TargetWindow(Point2d Point, Rect Window);
}

internal sealed record FrameAnalysis(long Timestamp, int Exposure, Point2d? Hole, Point2d? RodTip,
                                     double MaxBrightness, double ProbeBrightness);

internal sealed record PreviewFrame(Mat Frame, Rect Roi, HoleSet? Holes, Point2d? RodTip, Point2d? Target, Rect? ProbeWindow);

internal sealed record HoleMark(Point2d Center, double Radius);

internal sealed record HoleSet(HoleMark[] Marks, int ChainCount);
