using TrayGroups.Core.Configuration;

namespace TrayGroups.Host.Icons;

internal static class GroupIconFactory
{
    private const int MaxTiles = 4;

    /// <summary>
    /// Uses the group's chosen icon when set and loadable; otherwise composes a
    /// grid of up to four member icons.
    /// </summary>
    public static OwnedIcon Create(GroupConfig group)
    {
        if (!string.IsNullOrWhiteSpace(group.IconPath))
        {
            using var custom = IconLoader.Load(group.IconPath, group.IconPath);
            if (custom is not null)
            {
                return OwnedIcon.FromBitmap(custom);
            }
        }

        var tiles = group.Entries
            .Take(MaxTiles)
            .Select(entry => IconLoader.Load(entry.IconPath, entry.Target))
            .OfType<Bitmap>()
            .ToList();

        try
        {
            using var composed = Compose(tiles);
            return OwnedIcon.FromBitmap(composed);
        }
        finally
        {
            tiles.ForEach(tile => tile.Dispose());
        }
    }

    private static Bitmap Compose(List<Bitmap> tiles)
    {
        var canvas = new Bitmap(IconLoader.Size, IconLoader.Size);
        using var graphics = Graphics.FromImage(canvas);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        if (tiles.Count == 0)
        {
            using var fallback = SystemIcons.Application.ToBitmap();
            graphics.DrawImage(fallback, 0, 0, IconLoader.Size, IconLoader.Size);
            return canvas;
        }

        if (tiles.Count == 1)
        {
            graphics.DrawImage(tiles[0], 0, 0, IconLoader.Size, IconLoader.Size);
            return canvas;
        }

        const int tileSize = IconLoader.Size / 2;
        for (var i = 0; i < tiles.Count; i++)
        {
            graphics.DrawImage(tiles[i], (i % 2) * tileSize, (i / 2) * tileSize, tileSize, tileSize);
        }

        return canvas;
    }
}

