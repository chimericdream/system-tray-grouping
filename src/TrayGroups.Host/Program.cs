namespace TrayGroups.Host;

internal static class Program
{
    // Per-user mutex so a second launch exits instead of duplicating tray icons.
    private const string SingleInstanceMutexName = "TrayGroups.SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}
