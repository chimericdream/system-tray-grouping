namespace TrayGroups.Host;

/// <summary>
/// Ensures one TrayGroups process per user session. A second launch signals the
/// first one (to open settings) instead of adding duplicate tray icons.
/// </summary>
internal sealed class SingleInstance : IDisposable
{
    private const string MutexName = "TrayGroups.SingleInstance";
    private const string SignalName = "TrayGroups.ShowSettings";

    private readonly Mutex _mutex;
    private readonly EventWaitHandle _signal;
    private RegisteredWaitHandle? _registration;

    private SingleInstance(Mutex mutex, EventWaitHandle signal)
    {
        _mutex = mutex;
        _signal = signal;
    }

    /// <summary>Returns null when another instance already holds the lock.</summary>
    public static SingleInstance? TryAcquire()
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName, out var isFirst);
        if (!isFirst)
        {
            mutex.Dispose();
            return null;
        }

        return new SingleInstance(mutex, new EventWaitHandle(false, EventResetMode.AutoReset, SignalName));
    }

    /// <summary>Asks the running instance to show its settings window.</summary>
    public static void SignalRunningInstance()
    {
        if (EventWaitHandle.TryOpenExisting(SignalName, out var signal))
        {
            using (signal)
            {
                signal.Set();
            }
        }
    }

    /// <summary>Runs <paramref name="onSignal"/> on a thread-pool thread whenever a second launch happens.</summary>
    public void Listen(Action onSignal)
    {
        _registration = ThreadPool.RegisterWaitForSingleObject(
            _signal,
            (_, _) => onSignal(),
            state: null,
            Timeout.Infinite,
            executeOnlyOnce: false);
    }

    public void Dispose()
    {
        _registration?.Unregister(null);
        _signal.Dispose();
        _mutex.ReleaseMutex();
        _mutex.Dispose();
    }
}
