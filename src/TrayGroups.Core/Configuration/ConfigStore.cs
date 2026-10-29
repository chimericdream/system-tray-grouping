using System.Text.Json;

namespace TrayGroups.Core.Configuration;

/// <summary>Loads and saves the per-user configuration file.</summary>
public sealed class ConfigStore(string filePath)
{
    public const string AppFolderName = "TrayGroups";
    public const string FileName = "config.json";

    public string FilePath { get; } = filePath;

    /// <summary>Default location: <c>%APPDATA%\TrayGroups\config.json</c>.</summary>
    public static ConfigStore CreateDefault() =>
        new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppFolderName,
            FileName));

    /// <summary>Returns an empty configuration when the file does not exist yet.</summary>
    public TrayGroupsConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            return new TrayGroupsConfig();
        }

        return ConfigSerializer.Deserialize(File.ReadAllText(FilePath));
    }

    /// <summary>Writes the file atomically so a crash cannot leave a truncated config.</summary>
    public void Save(TrayGroupsConfig config)
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = FilePath + ".tmp";
        File.WriteAllText(tempPath, ConfigSerializer.Serialize(config));
        File.Move(tempPath, FilePath, overwrite: true);
    }

    /// <summary>
    /// Loads the configuration, or an empty one if the file is unreadable. A bad file
    /// is preserved with a <c>.bad</c> suffix instead of being overwritten.
    /// </summary>
    public TrayGroupsConfig LoadOrRecover()
    {
        try
        {
            return Load();
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            File.Copy(FilePath, FilePath + ".bad", overwrite: true);
            return new TrayGroupsConfig();
        }
    }
}
