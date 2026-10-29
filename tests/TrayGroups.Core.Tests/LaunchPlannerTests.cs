using TrayGroups.Core.Configuration;
using TrayGroups.Core.Launching;

namespace TrayGroups.Core.Tests;

public class LaunchPlannerTests
{
    [Fact]
    public void UsesShellExecuteAndExpandsTarget()
    {
        var variable = "TRAYGROUPS_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(variable, Path.GetTempPath().TrimEnd('\\'));
        try
        {
            var entry = new EntryConfig { Name = "Tool", Target = $@"%{variable}%\tool.exe", Arguments = "--flag" };

            var startInfo = LaunchPlanner.Plan(entry);

            Assert.True(startInfo.UseShellExecute);
            Assert.Equal(Path.Combine(Path.GetTempPath().TrimEnd('\\'), "tool.exe"), startInfo.FileName);
            Assert.Equal("--flag", startInfo.Arguments);
            Assert.Equal(Path.GetTempPath().TrimEnd('\\'), startInfo.WorkingDirectory);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Fact]
    public void SkipsWorkingDirectoryForShellTargets()
    {
        var startInfo = LaunchPlanner.Plan(new EntryConfig
        {
            Name = "Packaged",
            Target = @"shell:AppsFolder\Vendor.App_abc!App",
        });

        Assert.Equal(string.Empty, startInfo.WorkingDirectory);
    }
}
