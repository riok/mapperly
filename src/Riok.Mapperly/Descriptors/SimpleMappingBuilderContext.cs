using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilders;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext
{
    private readonly MapperDescriptor _descriptor;
    private readonly List<Diagnostic> _diagnostics;
    private readonly MapperConfiguration _configuration;

    public SimpleMappingBuilderContext(
        Compilation compilation,
        MapperConfiguration configuration,
        WellKnownTypes types,
        SymbolAccessor symbolAccessor,
        AttributeDataAccessor attributeAccessor,
        MapperDescriptor descriptor,
        List<Diagnostic> diagnostics,
        MappingBuilder mappingBuilder,
        ExistingTargetMappingBuilder existingTargetMappingBuilder
    )
    {
        Compilation = compilation;
        Types = types;
        SymbolAccessor = symbolAccessor;
        _configuration = configuration;
        _descriptor = descriptor;
        _diagnostics = diagnostics;
        MappingBuilder = mappingBuilder;
        ExistingTargetMappingBuilder = existingTargetMappingBuilder;
        AttributeAccessor = attributeAccessor;
    }

    protected SimpleMappingBuilderContext(SimpleMappingBuilderContext ctx)
        : this(
            ctx.Compilation,
            ctx._configuration,
            ctx.Types,
            ctx.SymbolAccessor,
            ctx.AttributeAccessor,
            ctx._descriptor,
            ctx._diagnostics,
            ctx.MappingBuilder,
            ctx.ExistingTargetMappingBuilder
        ) { }

    public Compilation Compilation { get; }

    public MapperAttribute MapperConfiguration => _configuration.Mapper;

    public WellKnownTypes Types { get; }

    public SymbolAccessor SymbolAccessor { get; }

    public AttributeDataAccessor AttributeAccessor { get; }

    protected MappingBuilder MappingBuilder { get; }

    protected ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; }

    public virtual bool IsConversionEnabled(MappingConversionType conversionType) =>
        MapperConfiguration.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location, params object[] messageArgs)
    {
        var syntaxNode = location?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        var nodeLocation = syntaxNode?.GetLocation();
        _diagnostics.Add(Diagnostic.Create(descriptor, nodeLocation ?? _descriptor.Syntax.GetLocation(), messageArgs));
    }

    protected MappingConfiguration ReadConfiguration(MappingConfigurationReference configRef) => _configuration.BuildFor(configRef);
}
