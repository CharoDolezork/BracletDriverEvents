using System.Globalization;
using System.IO.Ports;

namespace BracletDriver;

public enum RodStatus
{
    Unknown = -1,
    Idle = 0,
    Moving = 1,
    MovingToPoint = 2,
    Spinning = 3,
    Stopping = 4,
}

// Связь с платой привода
//
//   T ш1 ш2 ш3 скорость   цель в шагах (абсолютная) и скорость, начать движение
//   P                     запросить статус
//   S / B / R             останов плавный / резкий / продолжить
//
//   G x y z velocity
//   Q v1 v2 v3 a=5000

public sealed class RodPort : IDisposable
{
    private readonly ProgressLog _log;
    private readonly ManualResetEventSlim _moveDone = new(false);
    private readonly ManualResetEventSlim _statusReceived = new(false);

    private SerialPort? _port;
    private string _incoming = "";

    public RodPort(ProgressLog log) => _log = log;

    public bool IsOpen => _port is { IsOpen: true };

    public RodStatus Status { get; private set; } = RodStatus.Unknown;

    public int[] Position { get; private set; } = [];


    public bool Open(Options opt)
    {
        if (IsOpen) return true;

        var port = new SerialPort(opt.RodPortName, opt.RodBaudRate, Parity.None, 8, StopBits.One)
        {
            WriteTimeout = opt.RodWriteTimeoutMs,

            DtrEnable = true,
            RtsEnable = true,
        };
        port.DataReceived += OnDataReceived;

        try
        {
            port.Open();
            port.DiscardInBuffer();
        }
        catch (Exception ex)
        {
            port.Dispose();
            _log.Log($"Порт {opt.RodPortName} не открылся: {ex.Message}");
            return false;
        }

        _incoming = "";
        _port = port;

        _log.Log($"Порт {opt.RodPortName} открыт на {opt.RodBaudRate} бод.");
        return true;
    }

    public void Close()
    {
        if (_port is null) return;

        _port.DataReceived -= OnDataReceived;

        try
        {
            _port.Dispose();
        }
        catch (Exception ex)
        {
            _log.Log($"Порт закрылся с ошибкой: {ex.Message}");
        }

        _port = null;

        // Ждущий ход поток не должен зависнуть на закрытии порта.
        _moveDone.Set();
        _log.Log("Порт закрыт.");
    }

    // Отправляет команду на плату
    public void Send(string line)
    {
        if (_port is null)
        {
            _log.Log($"Команда «{line}» не отправлена: порт не открыт.");
            return;
        }

        try
        {
            _port.Write(line + "\n");
            _log.Log($"Команда «{line}» отправлена");
        }
        catch (Exception ex)
        {
            _log.Log($"Команда «{line}» не ушла: {ex.Message}");
        }
    }

    // Ожидает завершение хода
    public bool WaitMove(int timeoutMs) => _moveDone.Wait(timeoutMs);

    // Ход по заданным координатам
    public void MoveTo(int[] target, int speed)
    {
        _moveDone.Reset();
        Send($"T {string.Join(' ', target)} {speed}");
    }

    // Спрашивает статус и ждёт ответа, пришедшего после запроса. false — ответа не было.
    public bool QueryStatus(int timeoutMs)
    {
        _statusReceived.Reset();
        Send("P");
        return _statusReceived.Wait(timeoutMs);
    }

    public void StopSmooth() => Send("S");

    public void Brake() => Send("B");

    public void Resume() => Send("R");

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            // Кладем все что есть в порту на данный момент в буффер
            _incoming += ((SerialPort)sender).ReadExisting();

            int end;
            // Если есть /n то как минимум 1 команда пришла полностью. Обрабатываем ее
            while ((end = _incoming.IndexOf('\n')) >= 0)
            {
                // Берем до /n
                string line = _incoming[..end];
                // Остальное оставляем в буффере
                _incoming = _incoming[(end + 1)..];

                // Обрабатываем то что прислала плата
                if (line.Length > 0) Handle(line);
            }
        }
        catch (Exception ex)
        {
            _log.Log($"Ошибка чтения из порта: {ex.Message}");
        }
    }

    private void Handle(string line)
    {
        _log.Log($"Плата: {line}");

        // Отмечаем что ход закончен если пришло "u"
        if (string.Equals(line.TrimEnd('\r'), "u", StringComparison.OrdinalIgnoreCase)) _moveDone.Set();
        else UpdateStatus(line);
    }

    private void UpdateStatus(string line)
    {
        var numbers = new List<int>();
        // Разделяет строку на числа (P 100 100 100 0 => [P, 100, 100, 100, 0] )
        foreach (var part in line.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries))
            // Парсит только числа
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                numbers.Add(value);
        // Проверяет что хоть что то распарсилось и что статус валиден
        if (numbers.Count == 0 || numbers[^1] is < -1 or > 4) return;
        Status = (RodStatus)numbers[^1];
        // Обновляет позицию стержня
        if (numbers.Count > 1) Position = numbers.GetRange(0, numbers.Count - 1).ToArray();

        _statusReceived.Set();
    }

    public void Dispose()
    {
        Close();
        _moveDone.Dispose();
    }
}
