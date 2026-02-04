using System.Reflection;
using System.Text.RegularExpressions;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests.Configuration;

public class MapperTargetsTest
{
    [Fact]
    public void TargetsFileShouldContainCompilerVisibleProperties()
    {
        var properties = typeof(MapperAttribute)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(p => "Mapperly" + p.Name)
            .ToHashSet();

        var targetsFilePath = Path.Combine(SourcePaths.GetSolutionDirectory(), "src", "Riok.Mapperly", "Riok.Mapperly.targets");

        File.Exists(targetsFilePath).ShouldBeTrue($"File not found: {targetsFilePath}");

        var targetsContent = File.ReadAllText(targetsFilePath);
        var matches = Regex
            .Matches(targetsContent, "CompilerVisibleProperty Include=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToHashSet();

        // if this does not match,
        // likely a CompilerVisibleProperty is missing in the Riok.Mapperly.targets file
        // or one is left over which was removed in the MapperlyAttribute.
        matches.ShouldBe(properties, ignoreOrder: true);
    }
}
