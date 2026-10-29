namespace TrayGroups.Core.Discovery;

/// <summary>An installed app found by a scanner, before it is added to a group.</summary>
/// <param name="Name">Display name.</param>
/// <param name="Target">Path the entry will launch (usually a Start Menu shortcut).</param>
/// <param name="Publisher">Company name from the app's version info, when known.</param>
public sealed record DiscoveredApp(string Name, string Target, string? Publisher = null);
