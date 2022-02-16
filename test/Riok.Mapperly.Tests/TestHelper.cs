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

    public static string GenerateSingleMapperMethodBody(string source, TestHelperOptions? options = null)
    {
        return GenerateMapperMethodBodies(source, options)
            .Single()
            .Body;
    }

    public static string GenerateMapperMethodBody(
        string source,
        string methodName = TestSourceBuilder.DefaultMapMethodName,
        TestHelperOptions? options = null)
    {
        return GenerateMapperMethodBodies(source, options)
            .Single(x => x.Name == methodName)
            .Body;
    }

    public static IEnumerable<(string Name, string Body)> GenerateMapperMethodBodies(
        string source,
        TestHelperOptions? options = null)
    {
        options ??= TestHelperOptions.Default;

        var result = Generate(source, options).GetRunResult();

        if (!options.AllowDiagnostics)
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
