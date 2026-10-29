namespace TrayGroups.Core.Launching;

/// <summary>
/// Converts between absolute paths and environment-variable form so a config can be
/// shared between machines and users.
/// </summary>
public static class PathPortability
{
    private static readonly string[] CollapsibleVariables =
    [
        "ProgramFiles",
        "ProgramFiles(x86)",
        "ProgramW6432",
        "ProgramData",
        "SystemRoot",
        "LOCALAPPDATA",
        "APPDATA",
        "USERPROFILE",
    ];

    public static string Expand(string path) => Environment.ExpandEnvironmentVariables(path);

    public static string Collapse(string path) => Collapse(path, Environment.GetEnvironmentVariable);

    /// <summary>Replaces the longest matching known folder prefix with its variable.</summary>
    public static string Collapse(string path, Func<string, string?> getVariable)
    {
        string? bestName = null;
        string? bestValue = null;

        foreach (var name in CollapsibleVariables)
        {
            var value = getVariable(name)?.TrimEnd('\\', '/');
            if (string.IsNullOrEmpty(value) || !IsUnder(path, value))
            {
                continue;
            }

            if (bestValue is null || value.Length > bestValue.Length)
            {
                bestName = name;
                bestValue = value;
            }
        }

        return bestName is null || bestValue is null
            ? path
            : $"%{bestName}%{path[bestValue.Length..]}";
    }

    private static bool IsUnder(string path, string folder) =>
        path.Length > folder.Length
        && path.StartsWith(folder, StringComparison.OrdinalIgnoreCase)
        && (path[folder.Length] is '\\' or '/');
}
