using OpenCvSharp;

namespace BracletDriver;

// Кандидаты в отверстия через адаптивный порог: пиксель сравнивается не с одним числом на весь
// кадр, а со средней яркостью своего окна. Поэтому тени и блики не заливают маску целиком —
// выделяется только то, что заметно темнее своих соседей.
public static class AdaptiveCandidateDetector
{
    public static List<Candidate> Detect(Mat grayIn, Options opt)
    {
        // Пиксель темнее среднего по окну на AdaptiveC и больше — кандидат.
        using var binary = new Mat();
        Cv2.AdaptiveThreshold(grayIn, binary, 255, AdaptiveThresholdTypes.MeanC,
            ThresholdTypes.BinaryInv, opt.AdaptiveBlockSize, opt.AdaptiveC);

        using var labels = new Mat();
        using var stats = new Mat();
        using var centroids = new Mat();
        int count = Cv2.ConnectedComponentsWithStats(
            binary, labels, stats, centroids, PixelConnectivity.Connectivity8);

        var result = new List<Candidate>();
        for (int label = 1; label < count; label++)
        {
            int width = stats.At<int>(label, 2);
            int height = stats.At<int>(label, 3);
            int area = stats.At<int>(label, 4);
            if (area < opt.AdaptiveMinArea || area > opt.AdaptiveMaxArea) continue;

            // Отверстие — компактное круглое пятно, прожилки дерева — вытянутые штрихи.
            // Отсекаются и по отношению сторон рамки, и по тому, какую её долю занимает пятно.
            double aspect = (double)Math.Max(width, height) / Math.Min(width, height);
            if (aspect > opt.AdaptiveMaxAspect) continue;

            double fill = (double)area / (width * height);
            if (fill < opt.AdaptiveMinFill) continue;

            result.Add(new Candidate
            {
                Area = area,
                Center = new Point2d(centroids.At<double>(label, 0), centroids.At<double>(label, 1))
            });
        }

        return result;
    }
}
