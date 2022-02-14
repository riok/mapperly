using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Riok.Mapperly.Helpers;

internal static class DebuggerUtil
{
    [Conditional("DEBUG_SOURCE_GENERATOR")]
    internal static void AttachDebugger()
    {
        if (Debugger.IsAttached)
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Debugger.Launch();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // on macos we currently only support rider (make sure "Generate Shell Scripts" is enabled in the jetbrains toolbox app
            // and the generated scripts are in the path)
            Process.Start("rider", $"attach-to-process {Process.GetCurrentProcess().Id} \"{FindSolutionFile()}\"");
        }

        // wait for debugger to be attached (up to 30s)
        // this leaves time to manually attach it on linux or if the automatic attach didn't work
        for (var i = 0; i < 30 && !Debugger.IsAttached; i++)
        {
            Thread.Sleep(1000);
        }
    }

    private static string FindSolutionFile([CallerFilePath] string? callerFile = null)
    {
        var dir = Path.GetDirectoryName(callerFile)
            ?? throw new InvalidOperationException("could not resolve solution directory");
        do
        {
            var slnFiles = Directory.GetFiles(dir, "*.sln", SearchOption.TopDirectoryOnly);
            if (slnFiles.Length == 1)
                return slnFiles[0];

            dir = Path.GetDirectoryName(dir);
        }
        while (dir != null);

        throw new InvalidOperationException("Could not find solution");
    }
}
