using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Tests.Generator;

public class IncrementalGeneratorTest
{
    [Fact]
    public void AddingUnrelatedTypeDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.Mapping("string", "string");

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, RunReasons.New);

        var compilation2 = compilation1.AddSyntaxTrees(TestSourceBuilder.SyntaxTree("struct MyValue {}"));
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, RunReasons.Cached);
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
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var compilation2 = compilation1.AddSyntaxTrees(secondary);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, RunReasons.Cached, 0);
        AssertRunReasons(driver2, RunReasons.New, 1);
    }

    [Fact]
    public void AppendingUnrelatedTypeDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var newTree = syntaxTree.WithRootAndOptions(
            syntaxTree.GetCompilationUnitRoot().AddMembers(ParseMemberDeclaration("struct Foo {}")!),
            syntaxTree.Options
        );

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, RunReasons.Cached);
    }

    [Fact]
    public void ModifyingMappedTypeDoesRegenerateOriginal()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("partial B Map(A source);", "record A(int Value);", "record B(int Value);");
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, RunReasons.New);

        var recordDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .Members.OfType<RecordDeclarationSyntax>()
            .Single(x => x.Identifier.Text == "A");
        var updatedRecordDeclaration = ParseMemberDeclaration("record A(string Value)")!;

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(recordDeclaration, updatedRecordDeclaration);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, RunReasons.ModifiedSource);
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
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, RunReasons.New);

        var classDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .Members.OfType<ClassDeclarationSyntax>()
            .Single(x => x.Identifier.Text == TestSourceBuilderOptions.DefaultMapperClassName);
        var member = ParseMemberDeclaration("internal partial int BarToBaz(int value);")!;
        var updatedClass = classDeclaration.AddMembers(member);

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(classDeclaration, updatedClass);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, RunReasons.ModifiedSourceAndDiagnostics);
    }

    [Fact]
    public void ModifyingMapperDiagnosticsOnlyDoesRegenerateDiagnosticsOnly()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var classDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .Members.OfType<ClassDeclarationSyntax>()
            .Single(x => x.Identifier.Text == TestSourceBuilderOptions.DefaultMapperClassName);
        var member = ParseMemberDeclaration("[MapperIgnoreSource(\"not_found_updated\")] partial B Map(A source);")!;
        var updatedClass = classDeclaration.WithMembers(new SyntaxList<MemberDeclarationSyntax>(member));

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(classDeclaration, updatedClass);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, RunReasons.ModifiedDiagnostics);
    }

    [Fact]
    public void MapperDefaultsChangeShouldRegenerateAffectedMapper()
    {
        var mapperSource = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: MapperDefaults(EnumMappingStrategy = EnumMappingStrategy.ByValueCheckDefined)]

            [Mapper]
            public partial class AffectedMapper
            {
                public partial E2 Map(E1 source);
            }

            [Mapper]
            public partial class UnaffectedMapper
            {
                public partial B Map(A source);
            }

            record A();
            record B();
            enum E1 { value, value2 }
            enum E2 { Value }
            """
        );

        var syntaxTree1 = CSharpSyntaxTree.ParseText(mapperSource, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree1);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, RunReasons.New);

        var compilationUnit1 = (CompilationUnitSyntax)syntaxTree1.GetRoot();
        var attribute = Attribute(
            IdentifierName("MapperDefaults"),
            ParseAttributeArgumentList("(EnumMappingStrategy = EnumMappingStrategy.ByValue)")
        );
        var attributeList = AttributeList(SingletonSeparatedList(attribute));
        var compilationUnit2 = compilationUnit1.WithAttributeLists(new SyntaxList<AttributeListSyntax>(attributeList));
        var syntaxTree2 = syntaxTree1.WithRootAndOptions(compilationUnit2, syntaxTree1.Options);
        var compilation2 = compilation1.ReplaceSyntaxTree(syntaxTree1, syntaxTree2);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, RunReasons.Modified, 0);
        AssertRunReasons(driver2, RunReasons.ModifiedDefaults, 1);
    }

    private static void AssertRunReasons(GeneratorDriver driver, RunReasons reasons, int mapperIndex = 0)
    {
        var runResult = driver.GetRunResult().Results[0];
        if (mapperIndex == 0)
        {
            // compilation and defaults are built access all mappers and not per mapper,
            // only assert for the first mapper
            AssertRunReason(runResult, MapperGeneratorStepNames.BuildCompilationContext, mapperIndex, reasons.CompilationStep);
            AssertRunReason(runResult, MapperGeneratorStepNames.BuildMapperDefaults, mapperIndex, reasons.BuildMapperDefaultsStep);
        }

        AssertRunReason(runResult, MapperGeneratorStepNames.ReportDiagnostics, mapperIndex, reasons.ReportDiagnosticsStep);
        AssertRunReason(runResult, MapperGeneratorStepNames.BuildMappers, mapperIndex, reasons.BuildMappersStep);
        AssertRunReason(runResult, MapperGeneratorStepNames.ImplementationSourceOutput, mapperIndex, reasons.SourceOutputStep);
    }

    private static void AssertRunReason(
        GeneratorRunResult runResult,
        string stepName,
        int outputIndex,
        IncrementalStepRunReason expectedStepReason
    )
    {
        var actualStepReason = runResult.TrackedSteps[stepName].SelectMany(x => x.Outputs).ElementAt(outputIndex).Reason;
        actualStepReason.Should().Be(expectedStepReason, $"step {stepName} of mapper at index {outputIndex}");
    }

    private record RunReasons(
        IncrementalStepRunReason CompilationStep,
        IncrementalStepRunReason BuildMapperDefaultsStep,
        IncrementalStepRunReason BuildMappersStep,
        IncrementalStepRunReason ReportDiagnosticsStep,
        IncrementalStepRunReason SourceOutputStep
    )
    {
        public static readonly RunReasons New =
            new(
                IncrementalStepRunReason.New,
                IncrementalStepRunReason.New,
                IncrementalStepRunReason.New,
                IncrementalStepRunReason.New,
                IncrementalStepRunReason.New
            );

        public static readonly RunReasons Cached =
            new(
                // compilation step should always be modified as each time a new compilation is passed
                IncrementalStepRunReason.Modified,
                IncrementalStepRunReason.Unchanged,
                IncrementalStepRunReason.Cached,
                IncrementalStepRunReason.Cached,
                IncrementalStepRunReason.Cached
            );

        public static readonly RunReasons Modified = Cached with
        {
            BuildMapperDefaultsStep = IncrementalStepRunReason.Modified,
            ReportDiagnosticsStep = IncrementalStepRunReason.Modified,
            BuildMappersStep = IncrementalStepRunReason.Modified,
            // the input changed therefore new instead of modified
            SourceOutputStep = IncrementalStepRunReason.New,
        };

        public static readonly RunReasons ModifiedDiagnostics = Cached with
        {
            BuildMappersStep = IncrementalStepRunReason.Unchanged,
            ReportDiagnosticsStep = IncrementalStepRunReason.Modified,
        };

        public static readonly RunReasons ModifiedSource = Cached with
        {
            ReportDiagnosticsStep = IncrementalStepRunReason.Unchanged,
            BuildMappersStep = IncrementalStepRunReason.Modified,
            // the input changed therefore new instead of modified
            SourceOutputStep = IncrementalStepRunReason.New,
        };

        public static readonly RunReasons ModifiedSourceAndDiagnostics = Cached with
        {
            ReportDiagnosticsStep = IncrementalStepRunReason.Modified,
            BuildMappersStep = IncrementalStepRunReason.Modified,
            // the input changed therefore new instead of modified
            SourceOutputStep = IncrementalStepRunReason.New,
        };

        public static readonly RunReasons ModifiedDefaults = Cached with { BuildMapperDefaultsStep = IncrementalStepRunReason.Modified, };
    }
}
