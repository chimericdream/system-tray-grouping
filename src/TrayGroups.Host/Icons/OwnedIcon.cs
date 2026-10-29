using TrayGroups.Host.Native;

namespace TrayGroups.Host.Icons;

/// <summary>An <see cref="System.Drawing.Icon"/> plus the native handle that must be freed with it.</summary>
internal sealed class OwnedIcon : IDisposable
{
    private readonly IntPtr _handle;

    private OwnedIcon(Icon icon, IntPtr handle)
    {
        Icon = icon;
        _handle = handle;
    }

    public Icon Icon { get; }

    public static OwnedIcon FromBitmap(Bitmap bitmap)
    {
        var handle = bitmap.GetHicon();
        return new OwnedIcon(Icon.FromHandle(handle), handle);
    }

    public void Dispose()
    {
        Icon.Dispose();
        if (_handle != IntPtr.Zero)
        {
            NativeMethods.DestroyIcon(_handle);
        }
    }
}
