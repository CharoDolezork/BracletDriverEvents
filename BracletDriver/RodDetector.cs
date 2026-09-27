using OpenCvSharp;
using OpenCvSharp.XImgProc;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace BracletDriver;

public sealed class RodDetector : IDisposable
{
    private readonly Options _opt;
    private readonly ProgressLog _log;
    private readonly Mat _channel = new();
    private readonly Mat _mask = new();
    private readonly Mat _skeleton = new();
    private readonly Mat _openKernel;
    public RodDetector(Options opt, ProgressLog log)
    {
        _opt = opt;
        _log = log;
        _openKernel = Cv2.GetStructuringElement(
            MorphShapes.Ellipse, new Size(opt.MorphKernelSize, opt.MorphKernelSize));
    }

    public (Point2d? RodTip, double BrightShare) Detect(Mat frame, bool findTip)
    {
        Cv2.ExtractChannel(frame, _channel, _opt.RodChannel); // номер 1
        Cv2.Threshold(_channel, _mask, _opt.RodBrightThreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu); // Отцу

        double brightShare = Cv2.CountNonZero(_mask) / (double)(_mask.Rows * _mask.Cols);

        if (!findTip) return (null, brightShare);

        Cv2.MorphologyEx(_mask, _mask, MorphTypes.Open, _openKernel);
        Cv2.MorphologyEx(_mask, _mask, MorphTypes.Close, _openKernel);

        //FilterSmallComponents(_mask, _opt.RodMinComponentArea);

        CvXImgProc.Thinning(_mask, _skeleton, ThinningTypes.ZHANGSUEN);

        using var labels = new Mat();
        using var stats = new Mat();
        using var centroids = new Mat();
        int count = Cv2.ConnectedComponentsWithStats(
            _skeleton, labels, stats, centroids, PixelConnectivity.Connectivity8);

        return (FindTip(labels, stats, count), brightShare);
    }

    private Point2d? FindTip(Mat labels, Mat stats, int count)
    {
        var side = _opt.RodTipSide;
        bool vertical = side is Side.Up or Side.Down;

        double bestElongation = double.MinValue;
        int bestLabel = 0;

        for (int label = 1; label < count; label++)
        {
            int width = stats.At<int>(label, 2);
            int height = stats.At<int>(label, 3);

            double elongation = vertical
                ? (double)height / width
                : (double)width / height;

            if (elongation > bestElongation)
            {
                bestElongation = elongation;
                bestLabel = label;
            }
        }

        if (bestLabel == 0) return null;

        var tip = ExtremePoint(labels, stats, bestLabel, side);
        return new Point2d(tip.X, tip.Y);
    }

    private static Point ExtremePoint(Mat labels, Mat stats, int label, Side side)
    {
        int left = stats.At<int>(label, 0);
        int top = stats.At<int>(label, 1);
        int width = stats.At<int>(label, 2);
        int height = stats.At<int>(label, 3);

        if (side is Side.Up or Side.Down)
        {
            int y = side is Side.Up ? top : top + height - 1;
            for (int x = left; x < left + width; x++)
                if (labels.At<int>(y, x) == label) return new Point(x, y);
        }
        else
        {
            int x = side is Side.Left ? left : left + width - 1;
            for (int y = top; y < top + height; y++)
                if (labels.At<int>(y, x) == label) return new Point(x, y);
        }

        throw new InvalidOperationException($"На краю рамки компонента {label} нет его пикселей.");
    }

    private static void FilterSmallComponents(Mat mask, int minArea)
    {
        using var labels = new Mat();
        using var stats = new Mat();
        using var centroids = new Mat();
        int count = Cv2.ConnectedComponentsWithStats(
            mask, labels, stats, centroids, PixelConnectivity.Connectivity8);

        using var labelMask = new Mat();
        for (int label = 1; label < count; label++)
            if (stats.At<int>(label, 4) < minArea)
            {
                Cv2.InRange(labels, new Scalar(label), new Scalar(label), labelMask);
                mask.SetTo(0, labelMask);
            }
    }

    public void Dispose()
    {
        _channel.Dispose();
        _mask.Dispose();
        _skeleton.Dispose();
        _openKernel.Dispose();
    }
}
