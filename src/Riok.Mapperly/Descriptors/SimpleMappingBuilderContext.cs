using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Diagnostics;
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
    MapperDescriptor descriptor,
    UnsafeAccessorContext unsafeAccessorContext,
    DiagnosticCollection diagnostics,
    MappingBuilder mappingBuilder,
    ExistingTargetMappingBuilder existingTargetMappingBuilder
)
{
    private readonly MapperDescriptor _descriptor = descriptor;
    private readonly DiagnosticCollection _diagnostics = diagnostics;
    private readonly CompilationContext _compilationContext = compilationContext;
    private readonly MapperConfigurationReader _configurationReader = configurationReader;

    protected SimpleMappingBuilderContext(SimpleMappingBuilderContext ctx)
        : this(
            ctx._compilationContext,
            ctx._configurationReader,
            ctx.SymbolAccessor,
            ctx.AttributeAccessor,
            ctx._descriptor,
            ctx.UnsafeAccessorContext,
            ctx._diagnostics,
            ctx.MappingBuilder,
            ctx.ExistingTargetMappingBuilder
        ) { }

    public Compilation Compilation => _compilationContext.Compilation;

    public MapperAttribute MapperConfiguration => _configurationReader.Mapper;

    public WellKnownTypes Types => _compilationContext.Types;

    public bool Static => _descriptor.Static;

    public SymbolAccessor SymbolAccessor { get; } = symbolAccessor;

    public AttributeDataAccessor AttributeAccessor { get; } = attributeAccessor;

    public UnsafeAccessorContext UnsafeAccessorContext { get; } = unsafeAccessorContext;

    protected MappingBuilder MappingBuilder { get; } = mappingBuilder;

    protected ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; } = existingTargetMappingBuilder;

    public virtual bool IsConversionEnabled(MappingConversionType conversionType) =>
        MapperConfiguration.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location, params object[] messageArgs) =>
        _diagnostics.ReportDiagnostic(descriptor, location, messageArgs);

    protected MappingConfiguration ReadConfiguration(MappingConfigurationReference configRef) => _configurationReader.BuildFor(configRef);
}
