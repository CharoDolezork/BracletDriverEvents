using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

namespace BracletDriver;
// Тёмные пятна подходящей площади — кандидаты в отверстия.
public static class CandidateDetector
{
    public static List<Candidate> Detect(Mat grayIn, Options opt)
    {
        // Тёмные пиксели (<= DarkMax) — кандидаты.

        // Отцу бинаризация
        using var binary = new Mat();
        Cv2.Threshold(grayIn, binary, opt.DarkMax, 255, ThresholdTypes.BinaryInv);

        using var labels = new Mat();
        using var stats = new Mat();
        using var centroids = new Mat();
        int count = Cv2.ConnectedComponentsWithStats(
            binary, labels, stats, centroids, PixelConnectivity.Connectivity8);

        var result = new List<Candidate>();
        for (int label = 1; label < count; label++)
        {
            int area = stats.At<int>(label, 4);
            if (area < opt.MinArea || area > opt.MaxArea) continue;

            result.Add(new Candidate
            {
                Area = area,
                Center = new Point2d(centroids.At<double>(label, 0), centroids.At<double>(label, 1))
            });
        }

        return result;
    }
}

public sealed class Candidate
{
    public required Point2d Center { get; init; }
    public required double Area { get; init; }

    // Радиус круга той же площади — нужен только для отрисовки.
    public double Radius => Math.Sqrt(Area / Math.PI);
}


