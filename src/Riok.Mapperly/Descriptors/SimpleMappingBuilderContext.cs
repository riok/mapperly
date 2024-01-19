using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext(
    CompilationContext compilationContext,
    MapperConfigurationReader configurationReader,
    SymbolAccessor symbolAccessor,
    AttributeDataAccessor attributeAccessor,
    UnsafeAccessorContext unsafeAccessorContext,
    DiagnosticCollection diagnostics,
    MappingBuilder mappingBuilder,
    ExistingTargetMappingBuilder existingTargetMappingBuilder,
    Location diagnosticLocation
)
{
    private readonly DiagnosticCollection _diagnostics = diagnostics;
    private readonly CompilationContext _compilationContext = compilationContext;
    private readonly MapperConfigurationReader _configurationReader = configurationReader;
    private readonly Location _diagnosticLocation = diagnosticLocation;

    protected SimpleMappingBuilderContext(SimpleMappingBuilderContext ctx, Location? diagnosticLocation)
        : this(
            ctx._compilationContext,
            ctx._configurationReader,
            ctx.SymbolAccessor,
            ctx.AttributeAccessor,
            ctx.UnsafeAccessorContext,
            ctx._diagnostics,
            ctx.MappingBuilder,
            ctx.ExistingTargetMappingBuilder,
            diagnosticLocation ?? ctx._diagnosticLocation
        ) { }

    public Compilation Compilation => _compilationContext.Compilation;

    public MappingConfiguration Configuration { get; protected init; } = configurationReader.MapperConfiguration;

    public WellKnownTypes Types => _compilationContext.Types;

    public SymbolAccessor SymbolAccessor { get; } = symbolAccessor;

    public AttributeDataAccessor AttributeAccessor { get; } = attributeAccessor;

    public UnsafeAccessorContext UnsafeAccessorContext { get; } = unsafeAccessorContext;

    protected MappingBuilder MappingBuilder { get; } = mappingBuilder;

    protected ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; } = existingTargetMappingBuilder;

    public virtual bool IsConversionEnabled(MappingConversionType conversionType) =>
        Configuration.Mapper.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? symbolLocation, params object[] messageArgs) =>
        _diagnostics.ReportDiagnostic(descriptor, symbolLocation?.GetSyntaxLocation() ?? _diagnosticLocation, messageArgs);

    protected MappingConfiguration ReadConfiguration(MappingConfigurationReference configRef) =>
        _configurationReader.BuildFor(configRef, _diagnostics);
}
