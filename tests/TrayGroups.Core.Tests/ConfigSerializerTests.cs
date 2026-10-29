using TrayGroups.Core.Configuration;

namespace TrayGroups.Core.Tests;

public class ConfigSerializerTests
{
    [Fact]
    public void RoundTripsGroupsAndEntries()
    {
        var config = new TrayGroupsConfig
        {
            Groups =
            [
                new GroupConfig
                {
                    Id = "utilities",
                    Name = "Utilities",
                    Entries =
                    [
                        new EntryConfig { Name = "Notepad", Target = "%SystemRoot%\\notepad.exe" },
                    ],
                },
            ],
        };

        var result = ConfigSerializer.Deserialize(ConfigSerializer.Serialize(config));

        var group = Assert.Single(result.Groups);
        Assert.Equal("Utilities", group.Name);
        var entry = Assert.Single(group.Entries);
        Assert.Equal("%SystemRoot%\\notepad.exe", entry.Target);
        Assert.Null(entry.Arguments);
    }

    [Fact]
    public void RejectsNewerSchemaVersion()
    {
        var json = $$"""{ "schemaVersion": {{TrayGroupsConfig.CurrentSchemaVersion + 1}}, "groups": [] }""";

        Assert.Throws<NotSupportedException>(() => ConfigSerializer.Deserialize(json));
    }
}
