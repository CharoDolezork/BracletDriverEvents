using OpenCvSharp;

namespace BracletDriver;

// Сборка пятен в цепочки — ряды отверстий одного браслета.
public static class ChainFinder
{
    // От каждого свободного пятна пробуем все направления и оставляем самую длинную цепочку.
    public static List<Chain> Find(List<Candidate> cands, Options opt)
    {
        var chains = new List<Chain>();
        var used = new HashSet<Candidate>();
        var grid = new SpatialGrid(cands, opt.ChainSearchRadius);

        foreach (var start in cands)
        {
            if (used.Contains(start)) continue;

            Chain? best = null;

            foreach (var second in grid.Nearby(start))
            {
                if (second == start || used.Contains(second)) continue;
                if (start.Center.DistanceTo(second.Center) > opt.ChainSearchRadius) continue;

                var points = BuildChain(grid, used, start, second, opt);
                if (points.Count < opt.MinChainPoints) continue;

                var chain = ToChain(points);
                if (chain.AverageStep < opt.MinChainAverageStep || chain.AverageStep > opt.MaxChainAverageStep) continue;

                if (best == null || chain.Count > best.Count) best = chain;
            }
            if (best == null) continue;

            foreach (var c in best.Points) used.Add(c);
            chains.Add(best);
        }

        return chains;
    }

    // Цель наведения — крайнее отверстие самой длинной цепочки.
    public static Candidate? SelectTarget(List<Chain> chains)
    {
        Chain? longest = null;
        foreach (var chain in chains)
            if (longest is null || chain.Count > longest.Count)
                longest = chain;

        return longest?.Points[0];
    }


    private static List<Candidate> BuildChain(SpatialGrid grid, HashSet<Candidate> used, Candidate a, Candidate b, Options opt)
    {
        var forward = GrowChain(grid, used, a, b, opt);
        var backward = GrowChain(grid, used, b, a, opt);

        // Обе половины начинаются с той же пары в обратном порядке — в начало идёт только хвост backward.
        var chain = new List<Candidate>(backward.Count + forward.Count - 2);
        for (int k = backward.Count - 1; k >= 2; k--) chain.Add(backward[k]);
        chain.AddRange(forward);

        if (ShouldReverse(chain[0], chain[^1]))
            chain.Reverse();

        return chain;
    }

    // Идём вперёд от b в сторону последнего шага, выбирая самого «прямого» соседа
    private static List<Candidate> GrowChain(SpatialGrid grid, HashSet<Candidate> used, Candidate a, Candidate b, Options opt)
    {
        var chain = new List<Candidate> { a, b };
        var localUsed = new HashSet<Candidate> { a, b };

        var dir = Normalize(b.Center - a.Center);
        var current = b;

        while (true)
        {
            Candidate? next = null;
            double bestAngle = double.MaxValue;

            foreach (var c in grid.Nearby(current))
            {
                if (used.Contains(c) || localUsed.Contains(c)) continue;
                if (current.Center.DistanceTo(c.Center) > opt.ChainSearchRadius) continue;

                double areaRatio = Math.Max(c.Area, current.Area) / Math.Min(c.Area, current.Area);
                if (areaRatio > opt.MaxAreaRatio) continue;

                var nextDir = Normalize(c.Center - current.Center);
                double cos = Math.Clamp(dir.DotProduct(nextDir), -1, 1);
                double angle = Math.Acos(cos) * (180.0 / Math.PI);
                if (angle > opt.MaxTurnAngleDeg) continue;

                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    next = c;
                }
            }

            if (next == null) break;

            chain.Add(next);
            localUsed.Add(next);

            dir = Normalize(next.Center - current.Center);
            current = next;
        }

        return chain;
    }

    // Цепочка всегда развёрнута в одну сторону, иначе цель прыгала бы между её концами от кадра к кадру.
    private static bool ShouldReverse(Candidate first, Candidate last)
    {
        if (first.Center.X != last.Center.X)
            return first.Center.X > last.Center.X;

        return first.Center.Y > last.Center.Y;
    }

    private static Point2d Normalize(Point2d v)
    {
        double len = Math.Sqrt(v.X * v.X + v.Y * v.Y);
        return new Point2d(v.X / len, v.Y / len);
    }

    public static Chain ToChain(List<Candidate> points)
    {
        double sum = 0;
        for (int k = 0; k < points.Count - 1; k++)
            sum += points[k].Center.DistanceTo(points[k + 1].Center);

        return new Chain
        {
            Points = points,
            AverageStep = points.Count > 1 ? (int)Math.Round(sum / (points.Count - 1)) : 0
        };
    }

    // Ячейка размером в радиус поиска: соседи пятна лежат только в своей и восьми смежных ячейках.
    private sealed class SpatialGrid
    {
        private readonly double _cellSize;
        private readonly Dictionary<(int, int), List<Candidate>> _cells = new();

        public SpatialGrid(List<Candidate> cands, double cellSize)
        {
            _cellSize = cellSize;
            foreach (var c in cands)
            {
                var key = CellOf(c);
                if (!_cells.TryGetValue(key, out var list))
                    _cells[key] = list = new List<Candidate>();
                list.Add(c);
            }
        }

        private (int, int) CellOf(Candidate c) =>
            ((int)Math.Floor(c.Center.X / _cellSize), (int)Math.Floor(c.Center.Y / _cellSize));

        public IEnumerable<Candidate> Nearby(Candidate center)
        {
            var (cx, cy) = CellOf(center);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (_cells.TryGetValue((cx + dx, cy + dy), out var list))
                        foreach (var c in list)
                            yield return c;
        }
    }
}

// Цепочка отверстий и её средний шаг в пикселях.
public sealed class Chain
{
    public required List<Candidate> Points { get; init; }
    public required int AverageStep { get; init; }
    public int Count => Points.Count;
}
