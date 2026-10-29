using System.Diagnostics;
using TrayGroups.Core.Configuration;

namespace TrayGroups.Core.Launching;

public static class LaunchPlanner
{
    /// <summary>
    /// Builds the process start info for an entry. Uses the shell so shortcuts, documents,
    /// and <c>shell:</c> targets work the same as double-clicking them.
    /// </summary>
    public static ProcessStartInfo Plan(EntryConfig entry)
    {
        var target = PathPortability.Expand(entry.Target);

        var startInfo = new ProcessStartInfo(target)
        {
            UseShellExecute = true,
        };

        if (!string.IsNullOrWhiteSpace(entry.Arguments))
        {
            startInfo.Arguments = PathPortability.Expand(entry.Arguments);
        }

        var directory = Path.GetDirectoryName(target);
        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        {
            startInfo.WorkingDirectory = directory;
        }

        return startInfo;
    }
}
