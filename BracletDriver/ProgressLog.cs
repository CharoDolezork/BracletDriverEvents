namespace BracletDriver;


public sealed class ProgressLog
{
    private readonly TextBox _box;

    public ProgressLog(TextBox box) => _box = box;

    public void Log(string message)
    {
        if (_box.IsDisposed) return;

        if (!_box.IsHandleCreated)
        {
            Append(message);
            return;
        }

        try
        {
            if (_box.InvokeRequired) _box.BeginInvoke(new Action(() => Append(message)));
            else Append(message);
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void Append(string message) =>
        _box.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
}
