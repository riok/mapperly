using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public static class TestHelper
{
    private static readonly GeneratorDriverOptions _enableIncrementalTrackingDriverOptions =
        new(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true);

    public static Task<VerifyResult> VerifyGenerator(string source, TestHelperOptions? options = null, params object?[] args)
    {
        var driver = Generate(source, options);
        var verify = Verify(driver);

        if (args.Any())
        {
            verify.UseParameters(args);
        }

        return verify.ToTask();
    }

    public static MapperGenerationResult GenerateMapper(string source, TestHelperOptions? options = null)
    {
        options ??= TestHelperOptions.NoDiagnostics;

        var result = Generate(source, options).GetRunResult();

        var mapperClassImpl = result.GeneratedTrees
            .Single()
            .GetRoot() // compilation
            .ChildNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();
        var methods = mapperClassImpl
            .ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(x => new GeneratedMethod(x))
            .ToDictionary(x => x.Name);

        var groupedDiagnostics = result.Diagnostics
            .GroupBy(x => x.Descriptor.Id)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<Diagnostic>)x.ToList());
        var mapperResult = new MapperGenerationResult(result.Diagnostics, groupedDiagnostics, methods);
        if (options.AllowedDiagnostics != null)
        {
            mapperResult.Should().NotHaveDiagnostics(options.AllowedDiagnostics);
        }

        return mapperResult;
    }

    public static CSharpCompilation BuildCompilation(NullableContextOptions nullableOption, params SyntaxTree[] syntaxTrees)
    {
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Concat(
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(MapperGenerator).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(MapperAttribute).Assembly.Location)
                }
            );

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableOption);

        return CSharpCompilation.Create("Tests", syntaxTrees, references, compilationOptions);
    }

    public static GeneratorDriver GenerateTracked(Compilation compilation)
    {
        var generator = new MapperGenerator();

        var driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            driverOptions: _enableIncrementalTrackingDriverOptions
        );
        return driver.RunGenerators(compilation);
    }

    private static GeneratorDriver Generate(string source, TestHelperOptions? options)
    {
        options ??= TestHelperOptions.NoDiagnostics;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(options.LanguageVersion));
        var compilation = BuildCompilation(options.NullableOption, syntaxTree);
        var generator = new MapperGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }
}
