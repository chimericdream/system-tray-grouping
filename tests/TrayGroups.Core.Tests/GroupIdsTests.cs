using TrayGroups.Core.Configuration;

namespace TrayGroups.Core.Tests;

public class GroupIdsTests
{
    [Theory]
    [InlineData("Avast apps", "avast-apps")]
    [InlineData("  AI  ", "ai")]
    [InlineData("Dev / Tools!", "dev-tools")]
    [InlineData("???", "group")]
    public void CreatesSlugFromName(string name, string expected)
    {
        Assert.Equal(expected, GroupIds.Create(name, []));
    }

    [Fact]
    public void AppendsSuffixWhenIdIsTaken()
    {
        var id = GroupIds.Create("AI", ["ai", "ai-2"]);

        Assert.Equal("ai-3", id);
    }

    [Fact]
    public void ComparesExistingIdsCaseInsensitively()
    {
        Assert.Equal("ai-2", GroupIds.Create("AI", ["AI"]));
    }
}
