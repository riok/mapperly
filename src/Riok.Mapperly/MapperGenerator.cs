using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Output;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly;

[Generator]
public class MapperGenerator : IIncrementalGenerator
{
    public static readonly string MapperAttributeName = typeof(MapperAttribute).FullName!;
    public static readonly string MapperDefaultsAttributeName = typeof(MapperDefaultsAttribute).FullName!;
    public static readonly string UseStaticMapperName = typeof(UseStaticMapperAttribute).FullName!;
    public static readonly string UseStaticMapperGenericName = typeof(UseStaticMapperAttribute<>).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_SOURCE_GENERATOR
        DebuggerUtil.AttachDebugger();
#endif
        // report compilation diagnostics
        var compilationDiagnostics = context.CompilationProvider.SelectMany(
            static (compilation, _) => BuildCompilationDiagnostics(compilation)
        );
        context.ReportDiagnostics(compilationDiagnostics);

        var nestedCompilations = SyntaxProvider.GetNestedCompilations(context);

        // build the compilation context
        var compilationContext = context
            .CompilationProvider.Combine(nestedCompilations)
            .Combine(context.ParseOptionsProvider)
            .Select(
                static (c, _) =>
                {
                    var ((compilation, nestedCompilations), parseOptions) = c;
                    return new CompilationContext(
                        (CSharpCompilation)compilation,
                        ((CSharpParseOptions)parseOptions).LanguageVersion,
                        new WellKnownTypes(compilation),
                        nestedCompilations.ToImmutableArray(),
                        new FileNameBuilder()
                    );
                }
            )
            .WithTrackingName(MapperGeneratorStepNames.BuildCompilationContext);

        // build the assembly default configurations
        var mapperDefaultsAssembly = SyntaxProvider.GetMapperDefaultDeclarations(context);
        var mapperDefaults = compilationContext
            .Combine(mapperDefaultsAssembly)
            .Select(static (x, _) => BuildDefaults(x.Left, x.Right))
            .WithTrackingName(MapperGeneratorStepNames.BuildMapperDefaults);

        var useStaticMappers = SyntaxProvider
            .GetUseStaticMapperDeclarations(context)
            .Select(BuildStaticMappers)
            .WithTrackingName(MapperGeneratorStepNames.BuildUseStaticMappers);

        // extract the mapper declarations and build the descriptors
        var mappersAndDiagnostics = SyntaxProvider
            .GetMapperDeclarations(context)
            .Combine(compilationContext)
            .Combine(mapperDefaults)
            .Combine(useStaticMappers)
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(
                static (x, ct) =>
                    BuildDescriptor(x.Left.Left.Left.Right, x.Left.Left.Left.Left, x.Left.Left.Right, x.Left.Right, x.Right, ct)
            )
            .WhereNotNull();

        // output the diagnostics
        var diagnostics = mappersAndDiagnostics
            .Select(static (x, _) => x.Diagnostics)
            .WithTrackingName(MapperGeneratorStepNames.ReportDiagnostics);
        context.ReportDiagnostics(diagnostics);

        // output the mappers
        var mappers = mappersAndDiagnostics.Select(static (x, _) => x.Mapper).WithTrackingName(MapperGeneratorStepNames.BuildMappers);
        context.EmitMapperSource(mappers);
    }

    private static MapperAndDiagnostics? BuildDescriptor(
        CompilationContext compilationContext,
        MapperDeclaration mapperDeclaration,
        MapperConfiguration mapperDefaults,
        ImmutableArray<UseStaticMapperConfiguration> assemblyScopedStaticMappers,
        AnalyzerConfigOptionsProvider configOptionsProvider,
        CancellationToken cancellationToken
    )
    {
        var mapperAttributeSymbol = compilationContext.Types.TryGet(MapperAttributeName);
        if (mapperAttributeSymbol == null)
            return null;

        var symbolAccessor = new SymbolAccessor(compilationContext, mapperDeclaration.Symbol);
        if (!symbolAccessor.HasAttribute<MapperAttribute>(mapperDeclaration.Symbol))
            return null;

        try
        {
            var builder = new DescriptorBuilder(
                compilationContext,
                mapperDeclaration,
                symbolAccessor,
                mapperDefaults,
                assemblyScopedStaticMappers
            );
            var (descriptor, diagnostics) = builder.Build(cancellationToken);

            // Get editorconfig settings from the mapper's source file
            // Note: [*.g.cs] patterns are not supported due to Roslyn limitation
            // Users should configure settings in [*.cs] or [*] sections
            var generatedFileName = compilationContext.FileNameBuilder.Build(descriptor);
            var configOptions = configOptionsProvider.GetOptions(mapperDeclaration.Syntax.SyntaxTree);
            configOptions.TryGetValue("end_of_line", out var endOfLine);
            configOptions.TryGetValue("charset", out var charset);
            var sourceTextConfig = new SourceTextConfig(endOfLine, charset);

            // Validate editorconfig settings and add diagnostics for unknown values
            var location = mapperDeclaration.Syntax.GetLocation();
            var allDiagnostics = diagnostics.ToList();
            ValidateEditorConfigSettings(endOfLine, charset, location, allDiagnostics);

            var mapper = new MapperNode(generatedFileName, SourceEmitter.Build(descriptor, cancellationToken), sourceTextConfig);
            return new MapperAndDiagnostics(mapper, allDiagnostics.ToImmutableEquatableArray());
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static readonly HashSet<string> _validCharsets = new(StringComparer.OrdinalIgnoreCase)
    {
        "utf-8",
        "utf-8-bom",
        "utf-16be",
        "utf-16le",
    };

    private static readonly HashSet<string> _validEndOfLineValues = new(StringComparer.OrdinalIgnoreCase) { "lf", "crlf", "cr" };

    private static void ValidateEditorConfigSettings(string? endOfLine, string? charset, Location location, List<Diagnostic> diagnostics)
    {
        if (charset is not null and not "" && !_validCharsets.Contains(charset))
        {
            diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.UnknownEditorConfigCharset, location, charset));
        }

        if (endOfLine is not null and not "" && !_validEndOfLineValues.Contains(endOfLine))
        {
            diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.UnknownEditorConfigEndOfLine, location, endOfLine));
        }
    }

    private static MapperConfiguration BuildDefaults(CompilationContext compilationContext, IAssemblySymbol? assemblySymbol)
    {
        if (assemblySymbol == null)
            return MapperConfiguration.Default;

        var mapperDefaultsAttribute = compilationContext.Types.TryGet(MapperDefaultsAttributeName);
        if (mapperDefaultsAttribute == null)
            return MapperConfiguration.Default;

        var assemblyMapperDefaultsAttribute = SymbolAccessor
            .GetAttributesSkipCache(assemblySymbol, mapperDefaultsAttribute)
            .FirstOrDefault();
        return assemblyMapperDefaultsAttribute == null
            ? MapperConfiguration.Default
            : AttributeDataAccessor.Access<MapperDefaultsAttribute, MapperConfiguration>(assemblyMapperDefaultsAttribute);
    }

    private static IEnumerable<Diagnostic> BuildCompilationDiagnostics(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csCompilation)
        {
            yield return Diagnostic.Create(
                DiagnosticDescriptors.LanguageVersionNotSupported,
                null,
                "<not a valid C# version>",
                LanguageVersion.CSharp9.ToDisplayString()
            );
            yield break;
        }

        if (csCompilation.LanguageVersion < LanguageVersion.CSharp9)
        {
            yield return Diagnostic.Create(
                DiagnosticDescriptors.LanguageVersionNotSupported,
                null,
                csCompilation.LanguageVersion.ToDisplayString(),
                LanguageVersion.CSharp9.ToDisplayString()
            );
        }
    }

    private static ImmutableArray<UseStaticMapperConfiguration> BuildStaticMappers(
        ImmutableArray<AttributeData> attributeDataList,
        CancellationToken ct
    )
    {
        if (attributeDataList.IsDefaultOrEmpty)
        {
            return ImmutableArray<UseStaticMapperConfiguration>.Empty;
        }

        var configurations = ImmutableArray.CreateBuilder<UseStaticMapperConfiguration>(attributeDataList.Length);
        foreach (var attributeData in attributeDataList)
        {
            ct.ThrowIfCancellationRequested();

            var config = AttributeDataAccessor.Access<UseStaticMapperAttribute<object>, UseStaticMapperConfiguration>(attributeData);
            configurations.Add(config);
        }

        return configurations.ToImmutable();
    }
}
