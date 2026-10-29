using TrayGroups.Core.Launching;

namespace TrayGroups.Core.Tests;

public class PathPortabilityTests
{
    private static string? Lookup(string name) => name switch
    {
        "ProgramFiles" => @"C:\Program Files",
        "ProgramFiles(x86)" => @"C:\Program Files (x86)",
        "LOCALAPPDATA" => @"C:\Users\someone\AppData\Local",
        "USERPROFILE" => @"C:\Users\someone",
        _ => null,
    };

    [Theory]
    [InlineData(@"C:\Program Files\Vendor\app.exe", @"%ProgramFiles%\Vendor\app.exe")]
    [InlineData(@"c:\program files (x86)\Vendor\app.exe", @"%ProgramFiles(x86)%\Vendor\app.exe")]
    [InlineData(@"C:\Users\someone\AppData\Local\Tool\tool.exe", @"%LOCALAPPDATA%\Tool\tool.exe")]
    [InlineData(@"C:\Users\someone\bin\tool.exe", @"%USERPROFILE%\bin\tool.exe")]
    [InlineData(@"D:\Elsewhere\tool.exe", @"D:\Elsewhere\tool.exe")]
    public void CollapsesKnownFolders(string path, string expected)
    {
        Assert.Equal(expected, PathPortability.Collapse(path, Lookup));
    }

    [Fact]
    public void DoesNotCollapseSiblingFolderWithSharedPrefix()
    {
        var path = @"C:\Program Files Extra\app.exe";

        Assert.Equal(path, PathPortability.Collapse(path, Lookup));
    }

    [Fact]
    public void ExpandResolvesEnvironmentVariables()
    {
        var variable = "TRAYGROUPS_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(variable, "value");
        try
        {
            Assert.Equal(@"value\app.exe", PathPortability.Expand($@"%{variable}%\app.exe"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }
}
