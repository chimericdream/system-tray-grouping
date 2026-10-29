using System.Text;

namespace TrayGroups.Core.Discovery;

/// <summary>A set of discovered apps that share a publisher.</summary>
public sealed record PublisherSuggestion(string Publisher, IReadOnlyList<DiscoveredApp> Apps);

public static class AppSuggestions
{
    private static readonly string[] NoiseWords =
    [
        "uninstall", "uninstaller", "readme", "release notes", "documentation",
        "user guide", "manual", "license", "website", "web site", "help", "support",
    ];

    private static readonly string[] LegalSuffixes =
    [
        "incorporated", "inc", "corporation", "corp", "limited", "ltd", "llc", "gmbh",
        "co", "company", "sro", "ag", "sa", "bv", "plc",
    ];

    // "Software" is dropped when matching publishers but kept in the display name.
    private static readonly string[] MatchOnlySuffixes = [.. LegalSuffixes, "software"];

    /// <summary>True for shortcuts that are unlikely to be apps a user wants in a menu.</summary>
    public static bool IsNoise(string name) =>
        NoiseWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));

    /// <summary>Removes noise and duplicate targets, sorted by name.</summary>
    public static IReadOnlyList<DiscoveredApp> Clean(IEnumerable<DiscoveredApp> apps) =>
        apps
            .Where(app => !IsNoise(app.Name))
            .GroupBy(app => app.Target, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    /// <summary>
    /// Groups apps by normalized publisher and keeps publishers with at least
    /// <paramref name="minimumApps"/> apps. Largest suggestions come first.
    /// </summary>
    public static IReadOnlyList<PublisherSuggestion> ByPublisher(
        IEnumerable<DiscoveredApp> apps,
        int minimumApps = 2) =>
        apps
            .Where(app => !string.IsNullOrWhiteSpace(app.Publisher))
            .GroupBy(app => NormalizePublisher(app.Publisher!))
            .Where(group => group.Key.Length > 0 && group.Count() >= minimumApps)
            .Select(group => new PublisherSuggestion(
                MostCommonDisplayName(group),
                group.OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase).ToList()))
            .OrderByDescending(suggestion => suggestion.Apps.Count)
            .ThenBy(suggestion => suggestion.Publisher, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    /// <summary>
    /// Lowercases the publisher and strips punctuation and corporate suffixes, so
    /// "Avast Software s.r.o." and "AVAST Software" land in the same bucket.
    /// </summary>
    public static string NormalizePublisher(string publisher)
    {
        var cleaned = new StringBuilder();
        foreach (var c in publisher.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                cleaned.Append(c);
            }
            else if (c is not '.' and not ',')
            {
                cleaned.Append(' ');
            }
        }

        var words = cleaned.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        while (words.Count > 1 && MatchOnlySuffixes.Contains(words[^1]))
        {
            words.RemoveAt(words.Count - 1);
        }

        return string.Join(' ', words);
    }

    private static string MostCommonDisplayName(IEnumerable<DiscoveredApp> apps)
    {
        var raw = apps
            .GroupBy(app => app.Publisher!.Trim(), StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .First()
            .Key;

        var words = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (words.Count > 1 && LegalSuffixes.Contains(words[^1].Replace(".", "").Replace(",", "").ToLowerInvariant()))
        {
            words.RemoveAt(words.Count - 1);
        }

        return string.Join(' ', words).TrimEnd(',', '.');
    }
}
