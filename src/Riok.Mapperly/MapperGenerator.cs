using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Output;
using Riok.Mapperly.Symbols;
using Riok.Mapperly.Templates;

namespace Riok.Mapperly;

[Generator]
public class MapperGenerator : IIncrementalGenerator
{
    public static readonly string MapperAttributeName = typeof(MapperAttribute).FullName!;
    public static readonly string MapperDefaultsAttributeName = typeof(MapperDefaultsAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_SOURCE_GENERATOR
        DebuggerUtil.AttachDebugger();
#endif
        var assemblyName = context.CompilationProvider.Select((x, _) => x.Assembly.Name);

        // report compilation diagnostics
        var compilationDiagnostics = context
            .CompilationProvider
            .SelectMany(static (compilation, _) => BuildCompilationDiagnostics(compilation));
        context.ReportDiagnostics(compilationDiagnostics);

        // build the compilation context
        var compilationContext = context
            .CompilationProvider
            .Select(static (c, _) => new CompilationContext(c, new WellKnownTypes(c), new FileNameBuilder()))
            .WithTrackingName(MapperGeneratorStepNames.BuildCompilationContext);

        // build the assembly default configurations
        var mapperDefaultsAssembly = SyntaxProvider.GetMapperDefaultDeclarations(context);
        var mapperDefaults = compilationContext
            .Combine(mapperDefaultsAssembly)
            .Select(static (x, _) => BuildDefaults(x.Left, x.Right))
            .WithTrackingName(MapperGeneratorStepNames.BuildMapperDefaults);

        // extract the mapper declarations and build the descriptors
        var mappersAndDiagnostics = SyntaxProvider
            .GetMapperDeclarations(context)
            .Combine(compilationContext)
            .Combine(mapperDefaults)
            .Select(static (x, ct) => BuildDescriptor(x.Left.Right, x.Left.Left, x.Right, ct))
            .WhereNotNull();

        // output the diagnostics
        var diagnostics = mappersAndDiagnostics
            .Select(static (x, _) => x.Diagnostics)
            .WithTrackingName(MapperGeneratorStepNames.ReportDiagnostics);
        context.ReportDiagnostics(diagnostics);

        // output the mappers
        var mappers = mappersAndDiagnostics.Select(static (x, _) => x.Mapper).WithTrackingName(MapperGeneratorStepNames.BuildMappers);
        context.EmitMapperSource(mappers);

        // output the templates
        var templates = mappersAndDiagnostics
            .SelectMany(static (x, _) => x.Templates)
            .Collect()
            .SelectMany(static (x, _) => x.DistinctBy(tm => tm))
            .Combine(assemblyName)
            .WithTrackingName(MapperGeneratorStepNames.BuildTemplates)
            .Select(static (x, _) => TemplateReader.ReadContent(x.Left, x.Right))
            .WithTrackingName(MapperGeneratorStepNames.BuildTemplatesContent);
        context.EmitTemplates(templates);
    }

    private static MapperAndDiagnostics? BuildDescriptor(
        CompilationContext compilationContext,
        MapperDeclaration mapperDeclaration,
        MapperConfiguration mapperDefaults,
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
            var builder = new DescriptorBuilder(compilationContext, mapperDeclaration, symbolAccessor, mapperDefaults);
            var (descriptor, descriptorDiagnostics) = builder.Build(cancellationToken);
            var mapper = new MapperNode(
                compilationContext.FileNameBuilder.Build(descriptor),
                SourceEmitter.Build(descriptor, cancellationToken)
            );
            return new MapperAndDiagnostics(mapper, descriptorDiagnostics.ToImmutableEquatableArray(), descriptor.RequiredTemplates);
        }
        catch (OperationCanceledException)
        {
            return null;
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
        if (compilation is CSharpCompilation { LanguageVersion: < LanguageVersion.CSharp9 } cSharpCompilation)
        {
            yield return Diagnostic.Create(
                DiagnosticDescriptors.LanguageVersionNotSupported,
                null,
                cSharpCompilation.LanguageVersion.ToDisplayString(),
                LanguageVersion.CSharp9.ToDisplayString()
            );
        }
    }
}
