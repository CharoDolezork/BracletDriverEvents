using OpenCvSharp;
using System.Text.Json;

namespace BracletDriver;
internal sealed class MotionModel
{
    private const int Motors = SerialRodDriver.Motors;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly Options _opt;
    private readonly double[,] _table = new double[2, Motors];
    private readonly bool[] _calibrated = new bool[Motors];

    public MotionModel(Options opt) => _opt = opt;

    public bool IsCalibrated => _calibrated.All(c => c);

    public void SetColumn(int motor, Point2d shift, double probeStep)
    {
        _table[0, motor] = shift.X / probeStep;
        _table[1, motor] = shift.Y / probeStep;
        _calibrated[motor] = true;
    }

    public void Save(string path)
    {
        var rows = new double[2][];
        for (int row = 0; row < 2; row++)
        {
            rows[row] = new double[Motors];
            for (int k = 0; k < Motors; k++) rows[row][k] = _table[row, k];
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(new CalibrationFile(rows, DateTime.Now), JsonOptions));
    }

    public bool TryLoad(string path, out string? error)
    {
        error = null;
        if (!File.Exists(path))
        {
            error = "Файл калибровки не найден. Сначала нажмите «Калибровать моторы».";
            return false;
        }

        try
        {
            var data = JsonSerializer.Deserialize<CalibrationFile>(File.ReadAllText(path));
            if (data?.Table is not { Length: 2 } rows || rows.Any(row => row is null || row.Length != Motors))
            {
                error = $"Файл калибровки ({path}) повреждён. Нажмите «Калибровать моторы».";
                return false;
            }

            for (int row = 0; row < 2; row++)
                for (int k = 0; k < Motors; k++)
                    _table[row, k] = rows[row][k];

            Array.Fill(_calibrated, true);
            return true;
        }
        catch (Exception ex)
        {
            error = $"Не удалось прочитать файл калибровки ({path}): {ex.Message}";
            return false;
        }
    }

    public double[]? StepTo(Point2d error, double maxStep)
    {
        double m00 = 0, m01 = 0, m11 = 0;
        for (int k = 0; k < Motors; k++)
        {
            m00 += _table[0, k] * _table[0, k];
            m01 += _table[0, k] * _table[1, k];
            m11 += _table[1, k] * _table[1, k];
        }

        double det = m00 * m11 - m01 * m01;
        if (Math.Abs(det) < 1e-12) return null;

        double y0 = (m11 * error.X - m01 * error.Y) / det;
        double y1 = (-m01 * error.X + m00 * error.Y) / det;

        var step = new double[Motors];
        for (int k = 0; k < Motors; k++)
            step[k] = (_table[0, k] * y0 + _table[1, k] * y1) * _opt.GuidanceGain;

        double largest = step.Max(Math.Abs);
        if (maxStep > 0 && largest > maxStep)
            for (int k = 0; k < Motors; k++)
                step[k] *= maxStep / largest;

        return step;
    }

    public double[]? DescentStep(double length)
    {
        double nx = _table[0, 1] * _table[1, 2] - _table[0, 2] * _table[1, 1];
        double ny = _table[0, 2] * _table[1, 0] - _table[0, 0] * _table[1, 2];
        double nz = _table[0, 0] * _table[1, 1] - _table[0, 1] * _table[1, 0];

        double norm = Math.Sqrt(nx * nx + ny * ny + nz * nz);
        if (norm < 1e-12) return null;

        double scale = length * (int)_opt.GuidanceDescentSign / norm;
        return [nx * scale, ny * scale, nz * scale];
    }

    public bool Update(double[] applied, Point2d observed, out double residual)
    {
        double px = 0, py = 0, uu = 0;
        for (int k = 0; k < Motors; k++)
        {
            px += _table[0, k] * applied[k];
            py += _table[1, k] * applied[k];
            uu += applied[k] * applied[k];
        }

        double rx = observed.X - px;
        double ry = observed.Y - py;
        residual = Math.Sqrt(rx * rx + ry * ry);

        double shift = Math.Sqrt(observed.X * observed.X + observed.Y * observed.Y);
        if (uu == 0 || shift < _opt.GuidanceMinObservedShift || residual > _opt.GuidanceMaxResidual)
            return false;

        for (int k = 0; k < Motors; k++)
        {
            _table[0, k] += rx * applied[k] / uu;
            _table[1, k] += ry * applied[k] / uu;
        }

        return true;
    }

    public override string ToString()
    {
        var cols = Enumerable.Range(0, Motors).Select(k => $"м{k + 1}: ({_table[0, k]:F4}, {_table[1, k]:F4})");
        return "px на шаг — " + string.Join("; ", cols);
    }
}

internal sealed record CalibrationFile(double[][] Table, DateTime SavedAt);
