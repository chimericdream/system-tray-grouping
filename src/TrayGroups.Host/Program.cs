using TrayGroups.Core.Configuration;

namespace TrayGroups.Host;

internal static class Program
{
    /// <summary>Overrides the config file location (useful for portable installs and testing).</summary>
    private const string ConfigPathVariable = "TRAYGROUPS_CONFIG";

    [STAThread]
    private static void Main()
    {
        using var instance = SingleInstance.TryAcquire();
        if (instance is null)
        {
            SingleInstance.SignalRunningInstance();
            return;
        }

        var overridePath = Environment.GetEnvironmentVariable(ConfigPathVariable);
        var store = string.IsNullOrWhiteSpace(overridePath)
            ? ConfigStore.CreateDefault()
            : new ConfigStore(overridePath);

        ApplicationConfiguration.Initialize();
#pragma warning disable WFO5001 // Dark mode support is experimental in WinForms.
        Application.SetColorMode(SystemColorMode.System);
#pragma warning restore WFO5001

        Application.Run(new TrayApplicationContext(store, instance));
    }
}
