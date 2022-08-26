using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public static class TestHelper
{
    public static Task VerifyGenerator(
        string source,
        TestHelperOptions? options = null)
    {
        var driver = Generate(source, options);
        return Verify(driver).ToTask();
    }

    public static MapperGenerationResult GenerateMapper(string source, TestHelperOptions? options = null)
    {
        options ??= TestHelperOptions.Default;

        var result = Generate(source, options).GetRunResult();

        var mapperClassImpl = result.GeneratedTrees.Single()
            .GetRoot() // compilation
            .ChildNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();
        var methodBodies = mapperClassImpl
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .ToDictionary(m => m.Identifier.ToString(), ExtractBody);

        var mapperResult = new MapperGenerationResult(result.Diagnostics, methodBodies);
        if (options.AllowedDiagnostics != null)
        {
            mapperResult.Should().NotHaveDiagnostics(options.AllowedDiagnostics);
        }

        return mapperResult;
    }

    public static string GenerateSingleMapperMethodBody(string source, TestHelperOptions? options = null)
    {
        return GenerateMapperMethodBodies(source, options)
            .Single()
            .Value;
    }

    public static string GenerateMapperMethodBody(
        string source,
        string methodName = TestSourceBuilder.DefaultMapMethodName,
        TestHelperOptions? options = null)
    {
        return GenerateMapperMethodBodies(source, options)[methodName];
    }

    public static IReadOnlyDictionary<string, string> GenerateMapperMethodBodies(
        string source,
        TestHelperOptions? options = null)
    {
        return GenerateMapper(source, options ?? TestHelperOptions.NoDiagnostics).MethodBodies;
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
        TestHelperOptions? options)
    {
        options ??= TestHelperOptions.Default;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(options.LanguageVersion));
        var compilation = BuildCompilation(options.NullableOption, syntaxTree);
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
