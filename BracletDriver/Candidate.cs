using OpenCvSharp;

namespace BracletDriver;
internal sealed class Candidate
{
    public required Point2d Center { get; init; }
    public required double Area { get; init; }

    // Радиус круга той же площади — нужен только для отрисовки.
    public double Radius => Math.Sqrt(Area / Math.PI);
}
