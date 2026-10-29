using TrayGroups.Core.Configuration;

namespace TrayGroups.Core.Tests;

public sealed class ConfigStoreTests : IDisposable
{
    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), "traygroups-tests-" + Guid.NewGuid().ToString("N"));

    private ConfigStore CreateStore() => new(Path.Combine(_directory, "nested", ConfigStore.FileName));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void LoadReturnsEmptyConfigWhenFileIsMissing()
    {
        var config = CreateStore().Load();

        Assert.Empty(config.Groups);
    }

    [Fact]
    public void SaveCreatesDirectoryAndLoadReadsItBack()
    {
        var store = CreateStore();
        store.Save(new TrayGroupsConfig
        {
            Groups = [new GroupConfig { Id = "ai", Name = "AI" }],
        });

        var loaded = store.Load();

        Assert.Equal("AI", Assert.Single(loaded.Groups).Name);
        Assert.False(File.Exists(store.FilePath + ".tmp"));
    }

    [Fact]
    public void SaveOverwritesExistingFile()
    {
        var store = CreateStore();
        store.Save(new TrayGroupsConfig { Groups = [new GroupConfig { Id = "a", Name = "A" }] });
        store.Save(new TrayGroupsConfig { Groups = [new GroupConfig { Id = "b", Name = "B" }] });

        Assert.Equal("B", Assert.Single(store.Load().Groups).Name);
    }

    [Fact]
    public void LoadOrRecoverKeepsCorruptFileAndReturnsEmptyConfig()
    {
        var store = CreateStore();
        Directory.CreateDirectory(Path.GetDirectoryName(store.FilePath)!);
        File.WriteAllText(store.FilePath, "{ not json");

        var config = store.LoadOrRecover();

        Assert.Empty(config.Groups);
        Assert.Equal("{ not json", File.ReadAllText(store.FilePath + ".bad"));
    }
}
