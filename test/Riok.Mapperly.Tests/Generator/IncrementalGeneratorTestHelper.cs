using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Tests.Generator;

internal static class IncrementalGeneratorTestHelper
{
    public static CSharpCompilation ReplaceRecord(
        CSharpCompilation compilation,
        string recordName,
        [StringSyntax(StringSyntax.CSharp)] string newRecord
    )
    {
        var syntaxTree = compilation.SyntaxTrees.Single();
        var recordDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .Members
            .OfType<RecordDeclarationSyntax>()
            .Single(x => x.Identifier.Text == recordName);
        var updatedRecordDeclaration = SyntaxFactory.ParseMemberDeclaration(newRecord)!;

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(recordDeclaration, updatedRecordDeclaration);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        return compilation.ReplaceSyntaxTree(compilation.SyntaxTrees.First(), newTree);
    }

    public static void AssertRunReasons(GeneratorDriver driver, IncrementalGeneratorRunReasons reasons, int mapperIndex = 0)
    {
        var runResult = driver.GetRunResult().Results[0];
        if (mapperIndex == 0)
        {
            // compilation and defaults are built access all mappers and not per mapper,
            // only assert for the first mapper
            AssertRunReason(runResult, MapperGeneratorStepNames.BuildCompilationContext, reasons.CompilationStep, mapperIndex);
            AssertRunReason(runResult, MapperGeneratorStepNames.BuildMapperDefaults, reasons.BuildMapperDefaultsStep, mapperIndex);
        }

        AssertRunReason(runResult, MapperGeneratorStepNames.ReportDiagnostics, reasons.ReportDiagnosticsStep, mapperIndex);
        AssertRunReason(runResult, MapperGeneratorStepNames.BuildMappers, reasons.BuildMappersStep, mapperIndex);
    }

    public static void AssertRunReason(
        GeneratorDriver driver,
        string stepName,
        IncrementalStepRunReason expectedStepReason,
        int outputIndex = 0
    )
    {
        var runResult = driver.GetRunResult().Results[0];
        AssertRunReason(runResult, stepName, expectedStepReason, outputIndex);
    }

    private static void AssertRunReason(
        GeneratorRunResult runResult,
        string stepName,
        IncrementalStepRunReason expectedStepReason,
        int outputIndex
    )
    {
        var actualStepReason = runResult.TrackedSteps[stepName].SelectMany(x => x.Outputs).ElementAt(outputIndex).Reason;
        actualStepReason.Should().Be(expectedStepReason, $"step {stepName} of mapper at index {outputIndex}");
    }
}
