namespace BracletDriver;


public sealed class SerialRodDriver
{
    private const int Motors = 3;

    // Как часто переспрашивать статус
    private const int StatusPollMs = 100;

    // Сколько всего опрашивать статус
    private const int StatusFallbackMs = 2000;

    private readonly RodPort _port;
    private readonly Options _opt;
    private readonly ProgressLog _log;

    // Абсолютная позиция мотора
    private readonly int[] _sent = new int[Motors];
    // Шаг на который мотор следует сдвинуть
    private readonly double[] _applied = new double[Motors];

    public SerialRodDriver(RodPort port, Options opt, ProgressLog log)
    {
        _port = port;
        _opt = opt;
        _log = log;
    }

    // Ход закончен: true — плата подтвердила остановку, false — так и не ответила.
    // Вызывается в том потоке, который вызвал Step/GoTo, до того как они вернут управление.
    public event Action<bool>? MoveDone;

    public int MotorCount => Motors;

    public double[] Applied => _applied;

    // Отправленная позиция на плату
    public int[] Position => _sent;


    public bool TryStart()
    {
        if (!_port.IsOpen)
        {
            _log.Log("Плата не подключена.");
            return false;
        }

        // Ответ может прийти и без позиций (только статус) — поэтому длина проверяется отдельно.
        if (!_port.QueryStatus(StatusFallbackMs) || _port.Position.Length < Motors)
        {
            _log.Log($"Плата не сообщила позиции моторов (статус {_port.Status})");
            return false;
        }

        var position = _port.Position;
        for (int motor = 0; motor < Motors; motor++)
        {
            _sent[motor] = position[motor];
            _applied[motor] = 0;
        }

        _log.Log($"Исходные позиции моторов: {string.Join(", ", _sent)}.");
        return true;
    }


    public void Step(double[] step, int? speed = null)
    {
        for (int motor = 0; motor < Motors; motor++)
        {
            double s = step[motor];
            if (s != 0 && Math.Abs(s) < 1) s = Math.Sign(s);

            int applied = (int)Math.Round(s);
            _applied[motor] = applied;
            _sent[motor] += applied;
        }

        Move(speed);
    }

    public void GoTo(int[] position, int? speed = null)
    {
        for (int motor = 0; motor < Motors; motor++)
        {
            _applied[motor] = position[motor] - _sent[motor];
            _sent[motor] = position[motor];
        }

        Move(speed);
    }

    // Плавный останов. Можно вызывать из любого потока — идущий Step/GoTo дождётся остановки и вернётся.
    public void Stop() => _port.StopSmooth();

    // Отправляет ход и ждёт его окончания
    private void Move(int? speed)
    {
        _port.MoveTo(_sent, speed ?? _opt.RodSpeed);
        bool success = WaitIdle();

        try
        {
            MoveDone?.Invoke(success);
        }
        catch (Exception ex)
        {
            _log.Log($"Ошибка в обработчике окончания хода: {ex.Message}");
        }
    }

    // Ждём «U». false — плата так и не подтвердила, что стоит.
    private bool WaitIdle()
    {
        if (_port.WaitMove(_opt.RodMoveTimeoutMs)) return true;

        _log.Log("Плата не прислала «U» — спрашиваю статус.");

        long deadline = Environment.TickCount64 + StatusFallbackMs;
        while (Environment.TickCount64 < deadline)
        {
            if (_port.QueryStatus(StatusPollMs) && _port.Status == RodStatus.Idle) return true;

            // Пауза между запросами, заодно ловит «U», пришедшую с опозданием.
            if (_port.WaitMove(StatusPollMs)) return true;
        }

        _log.Log($"Плата не отвечает (статус {_port.Status}).");
        return false;
    }
}
