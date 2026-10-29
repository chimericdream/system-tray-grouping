using System.Runtime.ExceptionServices;

namespace TrayGroups.Host.Tests;

/// <summary>WinForms controls need an STA thread; xUnit runs tests on MTA threads.</summary>
internal static class StaThread
{
    public static void Run(Action action)
    {
        ExceptionDispatchInfo? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ExceptionDispatchInfo.Capture(ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        failure?.Throw();
    }
}
