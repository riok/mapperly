using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public static class TestHelper
{
    public static Task VerifyGenerator(
        string source,
        NullableContextOptions nullableOption = NullableContextOptions.Enable,
        LanguageVersion languageVersion = LanguageVersion.Default)
    {
        var driver = Generate(source, nullableOption, languageVersion);
        return Verify(driver).ToTask();
    }

    public static string GenerateSingleMapperMethodBody(string source, bool allowDiagnostics = false)
    {
        return GenerateMapperMethodBodies(source, allowDiagnostics)
            .Single()
            .Body;
    }

    public static IEnumerable<(string Name, string Body)> GenerateMapperMethodBodies(string source, bool allowDiagnostics = false)
    {
        var result = Generate(source).GetRunResult();

        if (!allowDiagnostics)
        {
            result.Diagnostics.Should().HaveCount(0);
        }

        var mapperClassImpl = result.GeneratedTrees.Single()
            .GetRoot() // compilation
            .ChildNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();
        return mapperClassImpl
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(methodImpl => (methodImpl.Identifier.ToString(), ExtractBody(methodImpl)));
    }

    private static string ExtractBody(MethodDeclarationSyntax methodImpl)
    {
        return methodImpl
            .Body
            ?.NormalizeWhitespace()
            .ToFullString()
            .Trim('{', '}', ' ', '\r', '\n') // simplify string to make assertions simpler
            .ReplaceLineEndings() ?? string.Empty;
    }

    private static GeneratorDriver Generate(
        string source,
        NullableContextOptions nullableOption = NullableContextOptions.Enable,
        LanguageVersion languageVersion = LanguageVersion.Default)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(languageVersion));
        var compilation = BuildCompilation(nullableOption, syntaxTree);
        var generator = new MapperGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }

    private static CSharpCompilation BuildCompilation(
        NullableContextOptions nullableOption,
        params SyntaxTree[] syntaxTrees)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(MapperGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MapperAttribute).Assembly.Location)
            });

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: nullableOption);

        return CSharpCompilation.Create(
            "Tests",
            syntaxTrees,
            references,
            compilationOptions);
    }
}
