using System.Diagnostics;

namespace BracletDriver;
internal static class Clock
{
    private static readonly Stopwatch Watch = Stopwatch.StartNew();

    public static long Ms => Watch.ElapsedMilliseconds;

    public static long After(int delayMs) => Watch.ElapsedMilliseconds + delayMs;

    public static bool Passed(long moment) => Watch.ElapsedMilliseconds >= moment;

    public static long Since(long moment) => Watch.ElapsedMilliseconds - moment;
}
