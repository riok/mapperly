using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.UnsafeAccess;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext(
    CompilationContext compilationContext,
    MapperDeclaration mapperDeclaration,
    MapperConfigurationReader configurationReader,
    SymbolAccessor symbolAccessor,
    GenericTypeChecker genericTypeChecker,
    AttributeDataAccessor attributeAccessor,
    UnsafeAccessorContext unsafeAccessorContext,
    DiagnosticCollection diagnostics,
    MappingBuilder mappingBuilder,
    ExistingTargetMappingBuilder existingTargetMappingBuilder,
    InlinedExpressionMappingCollection inlinedMappings,
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
            ctx.MapperDeclaration,
            ctx._configurationReader,
            ctx.SymbolAccessor,
            ctx.GenericTypeChecker,
            ctx.AttributeAccessor,
            ctx.UnsafeAccessorContext,
            ctx._diagnostics,
            ctx.MappingBuilder,
            ctx.ExistingTargetMappingBuilder,
            ctx.InlinedMappings,
            diagnosticLocation ?? ctx._diagnosticLocation
        )
    {
        // propagate DI settings and share cache fields dictionary
        ServiceProviderMemberName = ctx.ServiceProviderMemberName;
        RequestedDiMapperCacheFields = ctx.RequestedDiMapperCacheFields;
    }

    public MapperDeclaration MapperDeclaration { get; } = mapperDeclaration;

    public Compilation Compilation => _compilationContext.Compilation;

    public MappingConfiguration Configuration { get; protected init; } = configurationReader.MapperConfiguration;

    public WellKnownTypes Types => _compilationContext.Types;

    public SymbolAccessor SymbolAccessor { get; } = symbolAccessor;

    public GenericTypeChecker GenericTypeChecker { get; } = genericTypeChecker;

    public AttributeDataAccessor AttributeAccessor { get; } = attributeAccessor;

    public UnsafeAccessorContext UnsafeAccessorContext { get; } = unsafeAccessorContext;

    protected MappingBuilder MappingBuilder { get; } = mappingBuilder;

    protected ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; } = existingTargetMappingBuilder;

    /// <summary>
    /// The inline expression mappings.
    /// Note: No method mappings should be added to this collection
    /// and the body of these mappings is never built.
    /// </summary>
    protected InlinedExpressionMappingCollection InlinedMappings { get; } = inlinedMappings;

    /// <summary>
    /// Name of the mapper member annotated with [MapperServiceProvider] which exposes an IServiceProvider. Null if none.
    /// </summary>
    public string? ServiceProviderMemberName { get; set; }

    // Tracks requested cached DI mapper fields: fieldName -> constructed interface type (IMapper<S,T> or IExistingMapper<S,T>)
    internal Dictionary<string, INamedTypeSymbol> RequestedDiMapperCacheFields { get; } = [];

    public SemanticModel? GetSemanticModel(SyntaxTree syntaxTree) => _compilationContext.GetSemanticModel(syntaxTree);

    public virtual bool IsConversionEnabled(MappingConversionType conversionType) =>
        Configuration.Mapper.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? symbolLocation, params object[] messageArgs) =>
        _diagnostics.ReportDiagnostic(descriptor, symbolLocation?.GetSyntaxLocation() ?? _diagnosticLocation, messageArgs);

    protected MappingConfiguration ReadConfiguration(MappingConfigurationReference configRef, bool supportsDeepCloning) =>
        _configurationReader.BuildFor(configRef, supportsDeepCloning);

    public bool IsMappingNameEqualsTo(IMethodSymbol methodSymbol, string name)
    {
        var mappingName = AttributeAccessor.AccessFirstOrDefault<NamedMappingAttribute>(methodSymbol)?.Name ?? methodSymbol.Name;
        return string.Equals(mappingName, name, StringComparison.Ordinal);
    }
}
