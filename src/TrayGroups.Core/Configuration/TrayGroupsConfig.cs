namespace TrayGroups.Core.Configuration;

/// <summary>Root of the per-user configuration file.</summary>
public sealed record TrayGroupsConfig
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public IReadOnlyList<GroupConfig> Groups { get; init; } = [];
}

/// <summary>A named group that is shown as a single tray icon.</summary>
public sealed record GroupConfig
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    /// <summary>Optional path to a user-chosen icon. May contain environment variables.</summary>
    public string? IconPath { get; init; }

    public IReadOnlyList<EntryConfig> Entries { get; init; } = [];
}

/// <summary>An app shown in a group's menu.</summary>
public sealed record EntryConfig
{
    public required string Name { get; init; }

    /// <summary>Executable, shortcut, or shell target. May contain environment variables.</summary>
    public required string Target { get; init; }

    public string? Arguments { get; init; }

    /// <summary>Optional icon override; defaults to the target's own icon.</summary>
    public string? IconPath { get; init; }
}
