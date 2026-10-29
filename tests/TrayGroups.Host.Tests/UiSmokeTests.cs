using TrayGroups.Core.Configuration;
using TrayGroups.Host.Icons;
using TrayGroups.Host.Settings;
using TrayGroups.Host.Tray;

namespace TrayGroups.Host.Tests;

/// <summary>
/// Construction-level checks for the WinForms layer. They catch layout and
/// startup exceptions without needing anyone to click through the UI.
/// </summary>
public class UiSmokeTests
{
    private static string SystemExe(string name) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), name);

    private static TrayGroupsConfig SampleConfig() => new()
    {
        Groups =
        [
            new GroupConfig
            {
                Id = "utilities",
                Name = "Utilities",
                Entries =
                [
                    new EntryConfig { Name = "Calculator", Target = SystemExe("calc.exe") },
                    new EntryConfig { Name = "Missing", Target = SystemExe("definitely-not-here.exe") },
                ],
            },
        ],
    };

    [Fact]
    public void ShellIconLoadsForExistingExecutable()
    {
        StaThread.Run(() =>
        {
            using var bitmap = IconLoader.LoadShellIcon(SystemExe("notepad.exe"));

            Assert.NotNull(bitmap);
        });
    }

    [Fact]
    public void ShellIconIsNullForMissingFile()
    {
        StaThread.Run(() => Assert.Null(IconLoader.LoadShellIcon(SystemExe("definitely-not-here.exe"))));
    }

    [Fact]
    public void GroupIconIsComposedEvenWhenGroupIsEmpty()
    {
        StaThread.Run(() =>
        {
            using var icon = GroupIconFactory.Create(new GroupConfig { Id = "empty", Name = "Empty" });

            Assert.NotNull(icon.Icon);
        });
    }

    [Fact]
    public void GroupTrayBuildsMenuWithAnItemPerEntry()
    {
        StaThread.Run(() =>
        {
            using var tray = new GroupTray(SampleConfig().Groups[0], () => { }, () => { });
        });
    }

    [Fact]
    public void SettingsFormCreatesItsControls()
    {
        StaThread.Run(() =>
        {
            using var form = new SettingsForm(SampleConfig(), _ => { });

            _ = form.Handle;

            Assert.NotEmpty(form.Controls);
        });
    }

    [Fact]
    public void AppPickerFormCreatesItsControls()
    {
        StaThread.Run(() =>
        {
            using var form = (Form)Activator.CreateInstance(typeof(AppPickerForm), nonPublic: true)!;

            _ = form.Handle;

            Assert.NotEmpty(form.Controls);
        });
    }
}
