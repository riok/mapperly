using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Tests.Generator;

[UsesVerify]
public class IncrementalGeneratorTest
{
    [Fact]
    public void AddingUnrelatedTypeDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.Mapping("string", "string");

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(TestHelperOptions.NoDiagnostics.NullableOption, syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var compilation2 = compilation1.AddSyntaxTrees(TestSourceBuilder.SyntaxTree("struct MyValue {}"));
        var driver2 = driver1.RunGenerators(compilation2);

        AssertRunResults(MapperGenerator.AddMappersStep, driver2, IncrementalStepRunReason.Cached);
        AssertRunResults(MapperGenerator.ReportDiagnosticsStep, driver2, IncrementalStepRunReason.Cached);
    }

    [Fact]
    public void AddingNewMapperDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );

        var secondary = TestSourceBuilder.SyntaxTree(
            """
                    using Riok.Mapperly.Abstractions;

                    namespace Test.B
                    {
                        [Mapper]
                        internal partial class BarFooMapper
                        {
                            internal partial string BarToFoo(string value);
                        }
                    }
                    """
        );

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(TestHelperOptions.NoDiagnostics.NullableOption, syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var compilation2 = compilation1.AddSyntaxTrees(secondary);
        var driver2 = driver1.RunGenerators(compilation2);

        AssertRunResults(MapperGenerator.AddMappersStep, driver2, IncrementalStepRunReason.Cached, IncrementalStepRunReason.New);
        AssertRunResults(MapperGenerator.ReportDiagnosticsStep, driver2, IncrementalStepRunReason.Modified);
    }

    [Fact]
    public void AppendingUnrelatedTypeDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(TestHelperOptions.NoDiagnostics.NullableOption, syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var newTree = syntaxTree.WithRootAndOptions(
            syntaxTree.GetCompilationUnitRoot().AddMembers(SyntaxFactory.ParseMemberDeclaration("struct Foo {}")!),
            syntaxTree.Options
        );

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);

        AssertRunResults(MapperGenerator.AddMappersStep, driver2, IncrementalStepRunReason.Cached);
        AssertRunResults(MapperGenerator.ReportDiagnosticsStep, driver2, IncrementalStepRunReason.Cached);
    }

    [Fact]
    public void ModifyingMapperDoesRegenerateOriginal()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(TestHelperOptions.NoDiagnostics.NullableOption, syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var classDeclaration = syntaxTree.GetCompilationUnitRoot().Members.First() as ClassDeclarationSyntax;
        var member = SyntaxFactory.ParseMemberDeclaration("internal partial int BarToBaz(int value);")!;
        var updatedClass = classDeclaration!.AddMembers(member);

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(classDeclaration, updatedClass);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);

        AssertRunResults(MapperGenerator.AddMappersStep, driver2, IncrementalStepRunReason.New);
        AssertRunResults(MapperGenerator.ReportDiagnosticsStep, driver2, IncrementalStepRunReason.Modified);
    }

    private static void AssertRunResults(string name, GeneratorDriver driver, params IncrementalStepRunReason[] runReasons)
    {
        var runResult = driver.GetRunResult().Results[0];

        var step = runResult.TrackedSteps[name].SelectMany(x => x.Outputs);
        step.Select(x => x.Reason).Should().BeEquivalentTo(runReasons, o => o.WithStrictOrdering());
    }
}
