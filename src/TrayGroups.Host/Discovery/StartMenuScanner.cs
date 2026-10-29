using System.Diagnostics;
using TrayGroups.Core.Discovery;

namespace TrayGroups.Host.Discovery;

/// <summary>Finds installed apps by reading Start Menu shortcuts.</summary>
internal static class StartMenuScanner
{
    public static Task<IReadOnlyList<DiscoveredApp>> ScanAsync() => Task.Run(Scan);

    private static IReadOnlyList<DiscoveredApp> Scan()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
        };

        var apps = new List<DiscoveredApp>();
        var shell = CreateShell();

        foreach (var root in roots.Where(Directory.Exists))
        {
            foreach (var shortcut in EnumerateShortcuts(root))
            {
                var name = Path.GetFileNameWithoutExtension(shortcut);
                apps.Add(new DiscoveredApp(name, shortcut, ReadPublisher(shell, shortcut)));
            }
        }

        return AppSuggestions.Clean(apps);
    }

    private static IEnumerable<string> EnumerateShortcuts(string root) =>
        Directory.EnumerateFiles(
            root,
            "*.lnk",
            new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true });

    private static dynamic? CreateShell()
    {
        var type = Type.GetTypeFromProgID("WScript.Shell");
        return type is null ? null : Activator.CreateInstance(type);
    }

    private static string? ReadPublisher(dynamic? shell, string shortcutPath)
    {
        if (shell is null)
        {
            return null;
        }

        try
        {
            string? target = shell.CreateShortcut(shortcutPath).TargetPath;
            if (string.IsNullOrEmpty(target) || !File.Exists(target))
            {
                return null;
            }

            var company = FileVersionInfo.GetVersionInfo(target).CompanyName;
            return string.IsNullOrWhiteSpace(company) ? null : company.Trim();
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
