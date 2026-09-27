using DirectShowLib;
using OpenCvSharp;
using System.Diagnostics;
using Mat = OpenCvSharp.Mat;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;
using VideoCapture = OpenCvSharp.VideoCapture;
using VideoCaptureAPIs = OpenCvSharp.VideoCaptureAPIs;

namespace BracletDriver;

internal sealed class Camera : IDisposable
{
    // Для проверки что выдержку менять не надо
    private const int NoExposure = int.MinValue;
    private readonly ProgressLog _log;

    private VideoCapture? _videoCapture;
    private long _frameNumber;
    private int _pendingExposure = NoExposure;
    private int _currentExposure;

    private CancellationTokenSource? _readCts;
    private Task? _readLoop;

    public Camera(ProgressLog log) => _log = log;
    public event Action<CameraFrame>? FrameReceived;
    public bool IsOpen => _videoCapture?.IsOpened() ?? false;
    public long FrameNumber => Interlocked.Read(ref _frameNumber);
    public int CurrentExposure => Volatile.Read(ref _currentExposure);
    public void SetExposure(int value) => Volatile.Write(ref _pendingExposure, value);

    public bool Open(Options opt)
    {
        if (IsOpen) return true;

        int index = FindCameraIndexByName(opt.CameraNameFilter);
        if (index < 0)
        {
            _log.Log($"Камера с именем «{opt.CameraNameFilter}» не найдена");
            return false;
        }

        var vc = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
        if (!vc.IsOpened())
        {
            _log.Log($"Камера с индексом {index} не открылась");
            vc.Dispose();
            return false;
        }

        vc.FrameWidth = opt.CameraFrameWidth;
        vc.FrameHeight = opt.CameraFrameHeight;
        vc.Fps = opt.CameraFps;
        vc.AutoFocus = opt.CameraAutoFocus;
        if (!opt.CameraAutoFocus) vc.Focus = opt.CameraFocus;

        vc.Set(VideoCaptureProperties.AutoExposure, opt.CameraManualExposureValue);
        vc.Set(VideoCaptureProperties.AutoWB, 0);
        vc.Set(VideoCaptureProperties.Exposure, opt.CameraExposureStart);
        Volatile.Write(ref _currentExposure, (int)vc.Get(VideoCaptureProperties.Exposure));

        _videoCapture = vc;

        _readCts = new CancellationTokenSource();
        _readLoop = Task.Run(() => ReadLoop(vc, opt, _readCts.Token));
        return true;
    }

    public void Close()
    {
        _readCts?.Cancel();
        try
        {
            _readLoop?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
        }

        _readCts?.Dispose();
        _readCts = null;
        _readLoop = null;

        _videoCapture?.Dispose();
        _videoCapture = null;
    }

    public void Dispose() => Close();

    private void ReadLoop(VideoCapture capture, Options opt, CancellationToken token)
    {
        using var frame = new Mat();
        var resolved = Roi.Resolve(opt.Roi, new Size(opt.Roi.Width, opt.Roi.Height)); // исправить на просто Mat
        // Добавить в форму постройку ROI по 2-ум точкам (Кликам мышки по превью)
        int skipFrames = 0;

        while (!token.IsCancellationRequested)
        {
            try
            {

                int pending = Interlocked.Exchange(ref _pendingExposure, NoExposure);
                if (pending != NoExposure)
                {
                    capture.Set(VideoCaptureProperties.Exposure, pending);
                    int actual = (int)capture.Get(VideoCaptureProperties.Exposure);
                    Volatile.Write(ref _currentExposure, actual);
                    _log.Log($"Выдержка: просили {pending}, встало {actual}.");

                    skipFrames = opt.CameraExposureSettleFrames;
                }

                if (!capture.Read(frame) || frame.Empty())
                {
                    token.WaitHandle.WaitOne(opt.CameraRetryDelayMs);
                    continue;
                }

                long number = Interlocked.Increment(ref _frameNumber);

                if (skipFrames > 0)
                {
                    skipFrames--;
                    continue;
                }

                using var view = new Mat(frame, resolved);
                FrameReceived?.Invoke(new CameraFrame(view, number, Clock.Ms));
            }
            catch (Exception ex) 
            {
                _log.Log($"Ошибка в цикле чтения камеры: {ex.Message}");
                token.WaitHandle.WaitOne(opt.CameraRetryDelayMs);
            }
        }
    }

    private static int FindCameraIndexByName(string nameFilter)
    {
        var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        try
        {
            for (int index = 0; index < devices.Length; index++)
                if ((devices[index].Name ?? "").Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                    return index;

            return -1;
        }
        finally
        {
            foreach (var device in devices) device.Dispose();
        }
    }
}

public sealed record CameraFrame(Mat Frame, long FrameNumber, long Timestamp) : IDisposable
{
    public void Dispose() => Frame.Dispose();
}
