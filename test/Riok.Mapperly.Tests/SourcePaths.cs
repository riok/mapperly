using System.Runtime.CompilerServices;

namespace Riok.Mapperly.Tests;

internal static class SourcePaths
{
    private static string? _solutionDirectory;

    public static string GetSolutionDirectory([CallerFilePath] string callerFilePath = "")
    {
        if (_solutionDirectory != null)
            return _solutionDirectory;

        var directory = new DirectoryInfo(Path.GetDirectoryName(callerFilePath)!);
        while (directory != null && directory.GetFiles("*.slnx", SearchOption.TopDirectoryOnly).Length == 0)
        {
            directory = directory.Parent;
        }

        if (directory == null)
            throw new InvalidOperationException("Could not find solution directory (no .slnx file found)");

        return _solutionDirectory = directory.FullName;
    }
}
