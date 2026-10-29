using System.Text;

namespace TrayGroups.Core.Configuration;

public static class GroupIds
{
    /// <summary>Builds a lowercase, hyphenated id from a name that is not already in use.</summary>
    public static string Create(string name, IEnumerable<string> existingIds)
    {
        var taken = new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase);
        var slug = Slugify(name);

        if (!taken.Contains(slug))
        {
            return slug;
        }

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"{slug}-{suffix}";
            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    private static string Slugify(string name)
    {
        var builder = new StringBuilder();
        foreach (var c in name.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        return slug.Length == 0 ? "group" : slug;
    }
}
