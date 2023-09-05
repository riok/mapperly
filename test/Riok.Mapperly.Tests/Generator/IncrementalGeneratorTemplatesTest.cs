using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Riok.Mapperly.Tests.Generator.IncrementalGeneratorTestHelper;

namespace Riok.Mapperly.Tests.Generator;

public class IncrementalGeneratorTemplatesTest
{
    [Fact]
    public void ModifiedMapperShouldCacheTemplate()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithReferenceHandling,
            "record A(int Value);",
            "record B(int Value);"
        );
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);
        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReason(driver1, MapperGeneratorStepNames.BuildTemplates, IncrementalStepRunReason.New);
        AssertRunReason(driver1, MapperGeneratorStepNames.BuildTemplatesContent, IncrementalStepRunReason.New);

        var compilation2 = ReplaceRecord(compilation1, "A", "record A(string Value);");
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReason(driver2, MapperGeneratorStepNames.BuildTemplates, IncrementalStepRunReason.Cached);
        AssertRunReason(driver2, MapperGeneratorStepNames.BuildTemplatesContent, IncrementalStepRunReason.Cached);
    }
}
