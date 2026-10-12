using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace BracletDriver;
[Flags]
internal enum PreviewLayers
{
    None = 0,
    Roi = 1,
    Target = 2,
    Probe = 4,
    RodTip = 8,
    Holes = 16,
    All = Roi | Target | Probe | RodTip | Holes,
}

internal sealed class PreviewRenderer : IDisposable
{
    private const int MinIntervalMs = 1000 / 15;

    private readonly Options _opt;
    private readonly ProgressLog _log;
    private readonly Camera _camera;
    private readonly FrameAnalyzer _analyzer;

    private int _busy;
    private long _nextAt;

    private int _layers = (int)PreviewLayers.All;
    public PreviewLayers Layers
    {
        get => (PreviewLayers)Volatile.Read(ref _layers);
        set => Volatile.Write(ref _layers, (int)value);
    }

    public event Action<Bitmap>? PreviewReady;

    public PreviewRenderer(Options opt, ProgressLog log, Camera camera, FrameAnalyzer analyzer)
    {
        _opt = opt;
        _log = log;
        _camera = camera;
        _analyzer = analyzer;

        _camera.FrameReceived += OnFrameReceived;
        _analyzer.PreviewRequested += OnPreviewRequested;
    }

    private void OnFrameReceived(CameraFrame frame)
    {
        if (_analyzer.Enabled || !TryBegin()) return;

        var copy = frame.Frame.Clone();
        Task.Run(() => Publish(copy));
    }

    private void OnPreviewRequested(PreviewFrame frame)
    {
        if (!TryBegin()) return;

        var copy = frame with { Frame = frame.Frame.Clone() };
        Task.Run(() =>
        {
            Draw(copy);
            Publish(copy.Frame);
        });
    }

    private bool TryBegin()
    {
        if (!Clock.Passed(Volatile.Read(ref _nextAt))) return false;
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return false;

        Volatile.Write(ref _nextAt, Clock.After(MinIntervalMs));
        return true;
    }

    private void Draw(PreviewFrame frame)
    {
        var layers = Layers;

        if (layers.HasFlag(PreviewLayers.Roi))
            Cv2.Rectangle(frame.Frame, frame.Roi, _opt.BoxScalar, _opt.BoxThickness);

        if (layers.HasFlag(PreviewLayers.Holes) && frame.Holes is { } holes)
            foreach (var mark in holes.Marks)
                Cv2.Circle(frame.Frame, mark.Center.ToPoint(), Math.Max(1, (int)Math.Round(mark.Radius)),
                           _opt.HoleScalar, 1);

        if (layers.HasFlag(PreviewLayers.Probe) && frame.ProbeWindow is { } window)
            Cv2.Rectangle(frame.Frame, window, _opt.ProbeScalar, 1);

        if (layers.HasFlag(PreviewLayers.Target) && frame.Target is { } target)
            Cv2.DrawMarker(frame.Frame, target.ToPoint(), _opt.TargetScalar, MarkerTypes.Cross,
                           _opt.TipRadius * 4, _opt.BoxThickness);

        if (layers.HasFlag(PreviewLayers.RodTip) && frame.RodTip is { } tip)
            Cv2.Circle(frame.Frame, tip.ToPoint(), _opt.TipRadius, _opt.TipScalar, -1);
    }

    private void Publish(Mat frame)
    {
        try
        {
            PreviewReady?.Invoke(frame.ToBitmap());
        }
        catch (Exception ex)
        {
            _log.Log($"Ошибка превью: {ex.Message}");
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
        _analyzer.PreviewRequested -= OnPreviewRequested;
    }
}
