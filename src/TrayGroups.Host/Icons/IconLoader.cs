using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using TrayGroups.Core.Launching;
using TrayGroups.Host.Native;

namespace TrayGroups.Host.Icons;

internal static class IconLoader
{
    public const int Size = 32;

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    /// <summary>
    /// Loads an icon image for an entry: the explicit override if set, otherwise the
    /// target's own shell icon. Returns null when nothing usable is found.
    /// </summary>
    public static Bitmap? Load(string? iconOverride, string target)
    {
        if (!string.IsNullOrWhiteSpace(iconOverride))
        {
            var custom = LoadFromPath(PathPortability.Expand(iconOverride));
            if (custom is not null)
            {
                return custom;
            }
        }

        return LoadShellIcon(PathPortability.Expand(target));
    }

    public static Bitmap? LoadFromPath(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var extension = Path.GetExtension(path);
            if (ImageExtensions.Contains(extension))
            {
                using var image = Image.FromFile(path);
                return Resize(image);
            }

            if (extension.Equals(".ico", StringComparison.OrdinalIgnoreCase))
            {
                using var icon = new Icon(path, new Size(Size, Size));
                return icon.ToBitmap();
            }
        }
        catch (Exception ex) when (ex is OutOfMemoryException or ArgumentException or IOException)
        {
            return null;
        }

        return LoadShellIcon(path);
    }

    public static Bitmap? LoadShellIcon(string path)
    {
        var info = new NativeMethods.SHFILEINFO();
        var result = NativeMethods.SHGetFileInfo(
            path,
            0,
            ref info,
            (uint)Marshal.SizeOf<NativeMethods.SHFILEINFO>(),
            NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_LARGEICON);

        if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            using var icon = Icon.FromHandle(info.hIcon);
            return icon.ToBitmap();
        }
        finally
        {
            NativeMethods.DestroyIcon(info.hIcon);
        }
    }

    public static Bitmap Resize(Image source)
    {
        var bitmap = new Bitmap(Size, Size);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.DrawImage(source, 0, 0, Size, Size);
        return bitmap;
    }
}
