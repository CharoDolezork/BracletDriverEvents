using OpenCvSharp;
using Size = OpenCvSharp.Size;

namespace BracletDriver;

// Приведение настроенной рамки к конкретному кадру.
public static class Roi
{
    // Ненастроенная рамка — весь кадр; настроенная обрезается по его границам.
    public static Rect Resolve(Rect roi, Size frame)
    {
        if (roi.Width <= 0 || roi.Height <= 0)
            return new Rect(0, 0, frame.Width, frame.Height);

        int x = Math.Clamp(roi.X, 0, frame.Width);
        int y = Math.Clamp(roi.Y, 0, frame.Height);
        int width = Math.Clamp(roi.Width, 0, frame.Width - x);
        int height = Math.Clamp(roi.Height, 0, frame.Height - y);

        if (width <= 0 || height <= 0)
            return new Rect(0, 0, frame.Width, frame.Height);

        return new Rect(x, y, width, height);
    }
}
