using System.Diagnostics;

namespace BracletDriver;


public static class Clock
{
    private static readonly Stopwatch Watch = Stopwatch.StartNew();

    // Сколько миллисекунд прошло с запуска программы
    public static long Ms => Watch.ElapsedMilliseconds;

    // Отметка времени через delayMs миллисекунд: «не раньше этого момента»
    public static long After(int delayMs) => Watch.ElapsedMilliseconds + delayMs;

    // Прошёл ли момент, полученный из After
    public static bool Passed(long moment) => Watch.ElapsedMilliseconds >= moment;

    // Сколько прошло с отметки moment
    public static long Since(long moment) => Watch.ElapsedMilliseconds - moment;
}
