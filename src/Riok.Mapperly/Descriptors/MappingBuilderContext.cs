using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

[DebuggerDisplay("{GetType()}({Source.Name} => {Target.Name})")]
public class MappingBuilderContext : SimpleMappingBuilderContext
{
    private readonly IMethodSymbol? _userSymbol;
    private CollectionInfos? _collectionInfos;

    public MappingBuilderContext(
        SimpleMappingBuilderContext parentCtx,
        ObjectFactoryCollection objectFactories,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        MethodParameter[] parameters,
        ITypeSymbol target
    )
        : base(parentCtx)
    {
        ObjectFactories = objectFactories;
        Source = source;
        Target = target;
        Parameters = parameters;
        _userSymbol = userSymbol;
        Configuration = ReadConfiguration(_userSymbol);
    }

    protected MappingBuilderContext(
        MappingBuilderContext ctx,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        ITypeSymbol target,
        MethodParameter[] parameters
    )
        : this(ctx, ctx.ObjectFactories, userSymbol, source, parameters, target) { }

    public MappingConfiguration Configuration { get; }

    public ITypeSymbol Source { get; }

    public ITypeSymbol Target { get; }

    public MethodParameter[] Parameters { get; }

    public CollectionInfos? CollectionInfos => _collectionInfos ??= CollectionInfoBuilder.Build(Types, Source, Target);

    /// <summary>
    /// Whether the current mapping code is generated for a <see cref="System.Linq.Expressions.Expression"/>.
    /// </summary>
    public virtual bool IsExpression => false;

    public ObjectFactoryCollection ObjectFactories { get; }

    /// <inheritdoc cref="MappingBuilderContext.CallableUserMappings"/>
    public IReadOnlyCollection<IUserMapping> CallableUserMappings => MappingBuilder.CallableUserMappings;

    /// <summary>
    /// Tries to find an existing mapping for the provided types.
    /// If none is found, <c>null</c> is returned.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The found mapping, or <c>null</c> if none is found.</returns>
    public virtual ITypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType) =>
        MappingBuilder.Find(sourceType.UpgradeNullable(), targetType.UpgradeNullable());

    /// <summary>
    /// Tries to find an existing mapping for the provided types.
    /// If none is found, a new one is created.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further <see cref="FindOrBuildMapping"/> calls.
    /// No configuration / user symbol is passed.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public ITypeMapping? FindOrBuildMapping(ITypeSymbol sourceType, ITypeSymbol targetType, MethodParameter[] parameters) =>
        FindOrBuildMapping(null, sourceType, targetType, parameters, true);

    /// <summary>
    /// Tries to build a new mapping for the given types.
    /// The built mapping is not added to the mapping descriptor (should only be used as a delegate to another mapping)
    /// and is therefore not accessible by other mappings.
    /// Configuration / the user symbol is passed from the caller.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    /// <returns>The created mapping or <c>null</c> if none could be created.</returns>
    public ITypeMapping? BuildDelegateMapping(ITypeSymbol source, ITypeSymbol target, MethodParameter[] parameters) =>
        BuildMapping(_userSymbol, source, target, parameters, false);

    /// <summary>
    /// Tries to build a new mapping for the given types while keeping the current user symbol reference.
    /// This reuses configurations on the user symbol for the to be built mapping (eg. <see cref="Riok.Mapperly.Abstractions.MapPropertyAttribute"/>).
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further <see cref="FindOrBuildMapping"/> calls.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    /// <returns>The created mapping or <c>null</c> if none could be created.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ITypeMapping? BuildMappingWithUserSymbol(ITypeSymbol source, ITypeSymbol target, MethodParameter[] parameters) =>
        BuildMapping(
            _userSymbol
                ?? throw new InvalidOperationException(
                    nameof(BuildMappingWithUserSymbol) + " can only be called for contexts with a user symbol"
                ),
            source.UpgradeNullable(),
            target.UpgradeNullable(),
            parameters,
            true
        );

    /// <summary>
    /// Tries to find an existing mapping which can work with an existing target object instance for the provided types.
    /// If none is found, a new one is created.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further <see cref="FindOrBuildMapping"/> calls.
    /// No configuration / user symbol is passed.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual IExistingTargetMapping? FindOrBuildExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType) =>
        ExistingTargetMappingBuilder.Find(sourceType, targetType)
        ?? ExistingTargetMappingBuilder.Build(ContextForMapping(null, sourceType, Array.Empty<MethodParameter>(), targetType), true);

    /// <summary>
    /// Tries to build an existing target instance mapping.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further <see cref="FindOrBuildMapping"/> calls.
    /// No configuration / user symbol is passed.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual IExistingTargetMapping? BuildExistingTargetMappingWithUserSymbol(ITypeSymbol sourceType, ITypeSymbol targetType) =>
        ExistingTargetMappingBuilder.Build(ContextForMapping(_userSymbol, sourceType, Array.Empty<MethodParameter>(), targetType), false);

    protected virtual ITypeMapping? FindOrBuildMapping(
        IMethodSymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MethodParameter[] parameters,
        bool reusable
    )
    {
        sourceType = sourceType.UpgradeNullable();
        targetType = targetType.UpgradeNullable();
        return MappingBuilder.Find(sourceType, targetType) ?? BuildMapping(userSymbol, sourceType, targetType, parameters, reusable);
    }

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, params object[] messageArgs) =>
        base.ReportDiagnostic(descriptor, _userSymbol, messageArgs);

    public NullFallbackValue GetNullFallbackValue(ITypeSymbol? targetType = null) =>
        GetNullFallbackValue(targetType ?? Target, MapperConfiguration.ThrowOnMappingNullMismatch);

    protected virtual NullFallbackValue GetNullFallbackValue(ITypeSymbol targetType, bool throwOnMappingNullMismatch)
    {
        if (targetType.IsNullable())
            return NullFallbackValue.Default;

        if (throwOnMappingNullMismatch)
            return NullFallbackValue.ThrowArgumentNullException;

        if (!targetType.IsReferenceType)
            return NullFallbackValue.Default;

        if (targetType.SpecialType == SpecialType.System_String)
            return NullFallbackValue.EmptyString;

        if (targetType.HasAccessibleParameterlessConstructor())
            return NullFallbackValue.CreateInstance;

        ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, targetType);
        return NullFallbackValue.ThrowArgumentNullException;
    }

    protected virtual MappingBuilderContext ContextForMapping(
        IMethodSymbol? userSymbol,
        ITypeSymbol sourceType,
        MethodParameter[] parameters,
        ITypeSymbol targetType
    ) => new(this, userSymbol, sourceType, targetType, parameters);

    protected ITypeMapping? BuildMapping(
        IMethodSymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MethodParameter[] parameters,
        bool reusable
    ) =>
        MappingBuilder.Build(
            ContextForMapping(userSymbol, sourceType.UpgradeNullable(), parameters, targetType.UpgradeNullable()),
            reusable
        );
}
