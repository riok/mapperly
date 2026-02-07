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
        var mapperDefaultsAndDiagnostics = compilationContext
            .Combine(mapperDefaultsAssembly)
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (x, _) => BuildDefaults(x.Left.Left, x.Left.Right, x.Right.GlobalOptions))
            .WithTrackingName(MapperGeneratorStepNames.BuildMapperDefaults);
        context.ReportDiagnostics(mapperDefaultsAndDiagnostics.SelectMany(static (x, _) => x.Diagnostics));

        var mapperDefaults = mapperDefaultsAndDiagnostics.Select(static (x, _) => x.MapperConfiguration);
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
            .Select(static (x, ct) => BuildDescriptor(x.Left.Left.Right, x.Left.Left.Left, x.Left.Right, x.Right, ct))
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
        CancellationToken cancellationToken
    )
    {
        var symbolAccessor = new SymbolAccessor(compilationContext, mapperDeclaration.Symbol);
        var attributeDataAccessor = new AttributeDataAccessor(symbolAccessor);

        var mapperConfiguration = attributeDataAccessor.AccessFirstOrDefault<MapperAttribute, MapperConfiguration>(
            mapperDeclaration.Symbol
        );
        if (mapperConfiguration == null)
            return null;

        // if non-accessible members are included,
        // the compilation options need to be updated
        // and all symbols need to be mapped to the new compilation.
        var neededMetadata = GetNeededMetadataImportOptions(mapperConfiguration, mapperDefaults);
        if (neededMetadata != MetadataImportOptions.Public && neededMetadata > compilationContext.Compilation.Options.MetadataImportOptions)
        {
            // Enable access to private members
            var advancedOptions = compilationContext.Compilation.Options.WithMetadataImportOptions(neededMetadata);
            var compilation = compilationContext.Compilation.WithOptions(advancedOptions);

            // map all symbols to the new compilation
            compilationContext = compilationContext with
            {
                Compilation = compilation,
                Types = new WellKnownTypes(compilation),
            };
            assemblyScopedStaticMappers = MapStaticMappers(assemblyScopedStaticMappers, compilation);
            var mapperDeclarationSymbolFqn = mapperDeclaration.Symbol.FullyQualifiedMetadataName();
            var mappedMapperSymbol =
                compilationContext.Compilation.GetBestTypeByMetadataName(mapperDeclarationSymbolFqn)
                ?? throw new InvalidOperationException($"Could not get type {mapperDeclarationSymbolFqn}");

            mapperDeclaration = mapperDeclaration with { Symbol = mappedMapperSymbol };
            symbolAccessor = new SymbolAccessor(compilationContext, mappedMapperSymbol);
            attributeDataAccessor = new AttributeDataAccessor(symbolAccessor);
        }

        try
        {
            var builder = new DescriptorBuilder(
                compilationContext,
                mapperDeclaration,
                symbolAccessor,
                attributeDataAccessor,
                mapperDefaults,
                assemblyScopedStaticMappers
            );
            var (descriptor, diagnostics) = builder.Build(cancellationToken);
            var mapper = new MapperNode(
                compilationContext.FileNameBuilder.Build(descriptor),
                SourceEmitter.Build(descriptor, cancellationToken)
            );
            return new MapperAndDiagnostics(mapper, diagnostics.ToImmutableEquatableArray());
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static ImmutableArray<UseStaticMapperConfiguration> MapStaticMappers(
        ImmutableArray<UseStaticMapperConfiguration> staticMappers,
        CSharpCompilation compilation
    )
    {
        if (staticMappers.IsDefaultOrEmpty)
            return staticMappers;

        var staticMappersBuilder = ImmutableArray.CreateBuilder<UseStaticMapperConfiguration>(staticMappers.Length);
        foreach (var config in staticMappers)
        {
            var mapperTypeFqn = config.MapperType.FullyQualifiedMetadataName();
            var mappedMapperType =
                compilation.GetBestTypeByMetadataName(mapperTypeFqn)
                ?? throw new InvalidOperationException($"Could not get type {mapperTypeFqn}");
            staticMappersBuilder.Add(new UseStaticMapperConfiguration(mappedMapperType));
        }

        return staticMappersBuilder.ToImmutable();
    }

    private static (MapperConfiguration MapperConfiguration, ImmutableEquatableArray<Diagnostic> Diagnostics) BuildDefaults(
        CompilationContext compilationContext,
        IAssemblySymbol? assemblySymbol,
        AnalyzerConfigOptions options
    )
    {
        var (msbuildMapperConfiguration, diagnostics) = MapperBuildConfigurationReader.Read(options);

        if (assemblySymbol == null)
            return (msbuildMapperConfiguration, diagnostics);

        var mapperDefaultsAttribute = compilationContext.Types.TryGet(MapperDefaultsAttributeName);
        if (mapperDefaultsAttribute == null)
            return (msbuildMapperConfiguration, diagnostics);

        var assemblyMapperDefaultsAttribute = SymbolAccessor
            .GetAttributesSkipCache(assemblySymbol, mapperDefaultsAttribute)
            .FirstOrDefault();
        if (assemblyMapperDefaultsAttribute == null)
            return (msbuildMapperConfiguration, diagnostics);

        var attributeMapperConfiguration = AttributeDataAccessor.Access<MapperDefaultsAttribute, MapperConfiguration>(
            assemblyMapperDefaultsAttribute
        );
        var defaultMapperConfiguration = MapperConfigurationMerger.Merge(attributeMapperConfiguration, msbuildMapperConfiguration);
        return (defaultMapperConfiguration, diagnostics);
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

    private static MetadataImportOptions GetNeededMetadataImportOptions(
        MapperConfiguration mapperConfiguration,
        MapperConfiguration mapperDefaults
    )
    {
        var includedMembers = mapperConfiguration.IncludedMembers ?? mapperDefaults.IncludedMembers ?? MemberVisibility.AllAccessible;

        var includedConstructors =
            mapperConfiguration.IncludedConstructors ?? mapperDefaults.IncludedConstructors ?? MemberVisibility.AllAccessible;

        if (includedMembers.HasFlag(MemberVisibility.Accessible) && includedConstructors.HasFlag(MemberVisibility.Accessible))
        {
            return MetadataImportOptions.Public;
        }

        var visibility = includedMembers | includedConstructors;
        if (visibility.HasFlag(MemberVisibility.Private))
            return MetadataImportOptions.All;

        if (visibility.HasFlag(MemberVisibility.Internal) || visibility.HasFlag(MemberVisibility.Protected))
        {
            return MetadataImportOptions.Internal;
        }

        return MetadataImportOptions.Public;
    }
}
