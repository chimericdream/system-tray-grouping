using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrayGroups.Core.Configuration;

public static class ConfigSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(TrayGroupsConfig config) =>
        JsonSerializer.Serialize(config, Options);

    public static TrayGroupsConfig Deserialize(string json)
    {
        var config = JsonSerializer.Deserialize<TrayGroupsConfig>(json, Options)
            ?? throw new JsonException("Configuration is empty.");

        if (config.SchemaVersion > TrayGroupsConfig.CurrentSchemaVersion)
        {
            throw new NotSupportedException(
                $"Configuration schema version {config.SchemaVersion} is newer than supported version {TrayGroupsConfig.CurrentSchemaVersion}.");
        }

        return config;
    }
}
