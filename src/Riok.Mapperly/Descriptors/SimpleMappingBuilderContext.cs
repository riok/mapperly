using System.Diagnostics.CodeAnalysis;
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

    protected MappingConfiguration ReadConfiguration(MappingConfigurationReference configRef, bool supportsDeepCloning)
    {
        var result = ReadConfiguration([], configRef, supportsDeepCloning);
        if (result == null)
        {
            throw new InvalidOperationException("Failed to read configuration.");
        }

        return result;
    }

    private MappingConfiguration? ReadConfiguration(
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

        var includeMapping = AttributeAccessor.AccessFirstOrDefault<IncludeMappingConfigurationAttribute>(configRef.Method)?.Name;
        if (includeMapping != null)
        {
            var newInstanceMapping =
                (ITypeMapping?)MappingBuilder.FindOrResolveNamed(this, includeMapping, out var ambiguousName)
                ?? MappingBuilder.FindExistingInstanceNamedMapping(this, includeMapping, out ambiguousName);
            var methodSymbol = newInstanceMapping switch
            {
                UserDefinedNewInstanceMethodMapping udm => udm.Method,
                UserDefinedExistingTargetMethodMapping udm => udm.Method,
                _ => null,
            };

            if (!IsMappingValid(ambiguousName, configRef, newInstanceMapping, includeMapping))
            {
                return result;
            }

            if (methodSymbol == null)
            {
                _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, configRef.Method, includeMapping);
                return result;
            }

            if (!visitedMethods.Add(methodSymbol))
            {
                _diagnostics.ReportDiagnostic(
                    DiagnosticDescriptors.CircularReferencedMapping,
                    methodSymbol,
                    methodSymbol.ToDisplayString()
                );
                return null;
            }

            var newRef = new MappingConfigurationReference(methodSymbol, newInstanceMapping.SourceType, newInstanceMapping.TargetType);

            var result2 = ReadConfiguration(visitedMethods, newRef, supportsDeepCloning);
            return result2 != null ? result.MergeWith(result2) : result;
        }

        return result;
    }

    private bool IsMappingValid(
        bool ambiguousName,
        MappingConfigurationReference configRef,
        [NotNullWhen(true)] ITypeMapping? newInstanceMapping,
        string includeMapping
    )
    {
        if (ambiguousName)
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingAmbiguous, configRef.Method, includeMapping);
            return false;
        }

        if (newInstanceMapping is null)
        {
            _diagnostics.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, configRef.Method, includeMapping);
            return false;
        }

        var typeCheckerResult = GenericTypeChecker.InferAndCheckTypes(
            configRef.Method!.TypeParameters,
            (newInstanceMapping.SourceType, configRef.Source),
            (newInstanceMapping.TargetType, configRef.Target)
        );

        if (typeCheckerResult.Success)
        {
            return true;
        }

        if (ReferenceEquals(configRef.Source, typeCheckerResult.FailedArgument))
        {
            _diagnostics.ReportDiagnostic(
                DiagnosticDescriptors.SourceTypeIsNotAssignableToTheIncludedSourceType,
                configRef.Method,
                configRef.Source,
                newInstanceMapping.SourceType
            );
        }
        else
        {
            _diagnostics.ReportDiagnostic(
                DiagnosticDescriptors.TargetTypeIsNotAssignableToTheIncludedTargetType,
                configRef.Method,
                configRef.Target,
                newInstanceMapping.TargetType
            );
        }

        return false;
    }

    public IgnoreObsoleteMembersStrategy GetIgnoreObsoleteMembersStrategy()
    {
        return Configuration.Members.IgnoreObsoleteMembersStrategy.GetValueOrDefault(
            _configurationReader.MapperConfiguration.Members.IgnoreObsoleteMembersStrategy.GetValueOrDefault()
        );
    }
}
