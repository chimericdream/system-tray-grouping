using TrayGroups.Core.Discovery;

namespace TrayGroups.Core.Tests;

public class AppSuggestionsTests
{
    [Theory]
    [InlineData("Uninstall Widget", true)]
    [InlineData("Widget Help", true)]
    [InlineData("Widget", false)]
    public void DetectsNoise(string name, bool expected)
    {
        Assert.Equal(expected, AppSuggestions.IsNoise(name));
    }

    [Fact]
    public void CleanDropsNoiseAndDuplicateTargets()
    {
        DiscoveredApp[] apps =
        [
            new("Zeta", @"C:\a\zeta.lnk"),
            new("Alpha", @"C:\a\alpha.lnk"),
            new("Alpha (copy)", @"C:\A\ALPHA.lnk"),
            new("Uninstall Alpha", @"C:\a\un.lnk"),
        ];

        var cleaned = AppSuggestions.Clean(apps);

        Assert.Equal(["Alpha", "Zeta"], cleaned.Select(app => app.Name));
    }

    [Theory]
    [InlineData("Avast Software s.r.o.", "avast")]
    [InlineData("AVAST Software", "avast")]
    [InlineData("Contoso, Inc.", "contoso")]
    [InlineData("Software", "software")]
    public void NormalizesPublishers(string raw, string expected)
    {
        Assert.Equal(expected, AppSuggestions.NormalizePublisher(raw));
    }

    [Fact]
    public void GroupsByNormalizedPublisherAndSkipsSingletons()
    {
        DiscoveredApp[] apps =
        [
            new("Shield", "1", "Contoso Software s.r.o."),
            new("Cleanup", "2", "CONTOSO Software s.r.o."),
            new("Vpn", "3", "Contoso Software s.r.o."),
            new("Lonely", "4", "Other Corp"),
            new("Unknown", "5", null),
        ];

        var suggestions = AppSuggestions.ByPublisher(apps);

        var suggestion = Assert.Single(suggestions);
        Assert.Equal("Contoso Software", suggestion.Publisher);
        Assert.Equal(["Cleanup", "Shield", "Vpn"], suggestion.Apps.Select(app => app.Name));
    }
}
