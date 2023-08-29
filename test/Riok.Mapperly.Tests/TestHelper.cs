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

    public static MapperGenerationResult GenerateMapper(
        string source,
        TestHelperOptions? options = null,
        IReadOnlyCollection<TestAssembly>? additionalAssemblies = null,
        string? GeneratedTreeFileName = $"{TestSourceBuilderOptions.DefaultMapperClassName}.g.cs"
    )
    {
        options ??= TestHelperOptions.NoDiagnostics;

        var result = Generate(source, options, additionalAssemblies).GetRunResult();

        var methods = ExtractAllMethods(
                result.GeneratedTrees.SingleOrDefault(x => Path.GetFileName(x.FilePath) == GeneratedTreeFileName)?.GetRoot()
            )
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

    public static CSharpCompilation BuildCompilation(params SyntaxTree[] syntaxTrees) =>
        BuildCompilation("Tests", NullableContextOptions.Enable, true, syntaxTrees);

    public static TestAssembly BuildAssembly(string name, params SyntaxTree[] syntaxTrees)
    {
        var compilation = BuildCompilation(name, NullableContextOptions.Enable, false, syntaxTrees);
        return new TestAssembly(compilation);
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

    private static GeneratorDriver Generate(
        string source,
        TestHelperOptions? options,
        IReadOnlyCollection<TestAssembly>? additionalAssemblies = null
    )
    {
        options ??= TestHelperOptions.NoDiagnostics;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(options.LanguageVersion));
        var compilation = BuildCompilation(options.AssemblyName, options.NullableOption, true, syntaxTree);
        if (additionalAssemblies != null)
        {
            compilation = compilation.AddReferences(additionalAssemblies.Select(x => x.MetadataReference));
        }

        var generator = new MapperGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }

    private static CSharpCompilation BuildCompilation(
        string name,
        NullableContextOptions nullableOption,
        bool addMapperlyReferences,
        params SyntaxTree[] syntaxTrees
    )
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableOption);
        var compilation = CSharpCompilation.Create(name, syntaxTrees, options: compilationOptions);

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location));
        compilation = compilation.AddReferences(references);

        if (addMapperlyReferences)
        {
            compilation = compilation.AddReferences(
                MetadataReference.CreateFromFile(typeof(MapperGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MapperAttribute).Assembly.Location)
            );
        }

        return compilation;
    }

    private static IEnumerable<MethodDeclarationSyntax> ExtractAllMethods(SyntaxNode? root)
    {
        if (root == null)
            yield break;

        foreach (var node in root.ChildNodes())
        {
            // a namespace can contain classes
            if (node is NamespaceDeclarationSyntax)
            {
                foreach (var method in ExtractAllMethods(node))
                {
                    yield return method;
                }
            }

            // a class can contain methods or other classes
            if (node is not ClassDeclarationSyntax classNode)
                continue;

            foreach (var method in classNode.ChildNodes().OfType<MethodDeclarationSyntax>())
            {
                yield return method;
            }

            foreach (var method in ExtractAllMethods(node))
            {
                yield return method;
            }
        }
    }
}
