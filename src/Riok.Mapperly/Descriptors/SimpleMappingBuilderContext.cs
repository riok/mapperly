using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
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
    Location diagnosticLocation,
    SimpleMappingBuilderContext? childContext
)
{
    private readonly DiagnosticCollection _diagnostics = diagnostics;
    private readonly CompilationContext _compilationContext = compilationContext;
    private readonly MapperConfigurationReader _configurationReader = configurationReader;
    private readonly Location _diagnosticLocation = diagnosticLocation;

    protected SimpleMappingBuilderContext(
        SimpleMappingBuilderContext ctx,
        Location? diagnosticLocation,
        SimpleMappingBuilderContext? childContext
    )
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
            diagnosticLocation ?? ctx._diagnosticLocation,
            childContext ?? ctx.ChildContext
        ) { }

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
    /// Gets the child mapping context, if any.
    /// This is used to include mapping configurations from another mapper method.
    /// </summary>
    protected SimpleMappingBuilderContext? ChildContext { get; } = childContext;

    public SemanticModel? GetSemanticModel(SyntaxTree syntaxTree) => _compilationContext.GetSemanticModel(syntaxTree);

    public virtual bool IsConversionEnabled(MappingConversionType conversionType) =>
        Configuration.Mapper.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? symbolLocation, params object[] messageArgs) =>
        _diagnostics.ReportDiagnostic(descriptor, symbolLocation?.GetSyntaxLocation() ?? _diagnosticLocation, messageArgs);

    protected MappingConfiguration ReadConfiguration(
        HashSet<IMethodSymbol> visitedMethods,
        MappingConfigurationReference configRef,
        bool supportsDeepCloning
    )
    {
        var result = _configurationReader.BuildFor(configRef, supportsDeepCloning, _diagnostics);

        if (configRef.Method == null)
        {
            return result;
        }

        // TODO Inspect and merge derived type configurations
        var includeMapping = AttributeAccessor.AccessFirstOrDefault<IncludeMappingConfigurationAttribute>(configRef.Method)?.Name;
        if (includeMapping != null)
        {
            var newInstanceMapping =
                (ITypeMapping?)MappingBuilder.FindOrResolveNamed(this, includeMapping, out var ambiguousName)
                ?? MappingBuilder.FindExistingInstanceNamedMapping(this, includeMapping, out ambiguousName);
            if (ambiguousName || newInstanceMapping is null)
            {
                return result;
            }
            var udMapping = newInstanceMapping switch
            {
                UserDefinedNewInstanceMethodMapping udm => udm.Method,
                UserDefinedExistingTargetMethodMapping udm => udm.Method,
                _ => null,
            };
            if (udMapping == null || !visitedMethods.Add(configRef.Method))
            {
                _diagnostics.ReportDiagnostic(
                    DiagnosticDescriptors.CircularReferencedMapping,
                    udMapping,
                    udMapping?.ToDisplayString() ?? "<unknown>"
                );
                return result;
            }

            var newRef = new MappingConfigurationReference(udMapping, newInstanceMapping.SourceType, newInstanceMapping.TargetType);
            var result2 = ReadConfiguration(visitedMethods, newRef, supportsDeepCloning);
            return result.MergeWith(result2);
        }
        return result;
    }
}
