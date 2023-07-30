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

[DebuggerDisplay("{GetType().Name}({Source.Name} => {Target.Name})")]
public class MappingBuilderContext : SimpleMappingBuilderContext
{
    private CollectionInfos? _collectionInfos;

    public MappingBuilderContext(
        SimpleMappingBuilderContext parentCtx,
        ObjectFactoryCollection objectFactories,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        ITypeSymbol target,
        ImmutableEquatableArray<MethodParameter> parameters
    )
        : base(parentCtx)
    {
        ObjectFactories = objectFactories;
        Source = source;
        Target = target;
        Parameters = parameters;
        UserSymbol = userSymbol;
        Configuration = ReadConfiguration(new MappingConfigurationReference(UserSymbol, source, target));
    }

    protected MappingBuilderContext(
        MappingBuilderContext ctx,
        IMethodSymbol? userSymbol,
        ITypeSymbol source,
        ITypeSymbol target,
        bool clearDerivedTypes
    )
        : this(ctx, ctx.ObjectFactories, userSymbol, source, target, ImmutableEquatableArray<MethodParameter>.Empty)
    {
        if (clearDerivedTypes)
        {
            Configuration = Configuration with { DerivedTypes = Array.Empty<DerivedTypeMappingConfiguration>() };
        }
    }

    public MappingConfiguration Configuration { get; }

    public ITypeSymbol Source { get; }

    public ITypeSymbol Target { get; }

    public ImmutableEquatableArray<MethodParameter> Parameters { get; } = ImmutableEquatableArray.Empty<MethodParameter>();

    public CollectionInfos? CollectionInfos => _collectionInfos ??= CollectionInfoBuilder.Build(Types, SymbolAccessor, Source, Target);

    protected IMethodSymbol? UserSymbol { get; }

    /// <summary>
    /// Whether the current mapping code is generated for a <see cref="System.Linq.Expressions.Expression"/>.
    /// </summary>
    public virtual bool IsExpression => false;

    public ObjectFactoryCollection ObjectFactories { get; }

    /// <inheritdoc cref="MappingBuilders.MappingBuilder.UserMappings"/>
    public IReadOnlyCollection<IUserMapping> UserMappings => MappingBuilder.UserMappings;

    /// <summary>
    /// Tries to find an existing mapping for the provided types.
    /// If none is found, <c>null</c> is returned.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The found mapping, or <c>null</c> if none is found.</returns>
    public virtual ITypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType) =>
        MappingBuilder.Find(sourceType.UpgradeNullable(), targetType.UpgradeNullable(), Parameters);

    //TODO: updagrade nullable

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
    /// <param name="options">The mapping building options.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual ITypeMapping? FindOrBuildMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    )
    {
        sourceType = sourceType.UpgradeNullable();
        targetType = targetType.UpgradeNullable();
        return MappingBuilder.Find(sourceType, targetType, Parameters) ?? BuildMapping(sourceType, targetType, options);
    }

    /// <summary>
    /// Builds a new mapping for the provided types with the given options.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    /// <param name="options">The options.</param>
    /// <returns>The created mapping, or <c>null</c> if no mapping could be created.</returns>
    public ITypeMapping? BuildMapping(ITypeSymbol source, ITypeSymbol target, MappingBuildingOptions options)
    {
        var userSymbol = options.HasFlag(MappingBuildingOptions.KeepUserSymbol) ? UserSymbol : null;
        return BuildMapping(userSymbol, source, target, options);
    }

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
    /// <param name="options">The options.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual IExistingTargetMapping? FindOrBuildExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    ) =>
        ExistingTargetMappingBuilder.Find(sourceType, targetType, Parameters)
        ?? BuildExistingTargetMapping(sourceType, targetType, options);

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
    /// <param name="options">The options.</param>
    /// <returns>The created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual IExistingTargetMapping? BuildExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    )
    {
        var userSymbol = options.HasFlag(MappingBuildingOptions.KeepUserSymbol) ? UserSymbol : null;
        var ctx = ContextForMapping(userSymbol, sourceType, targetType, options);
        return ExistingTargetMappingBuilder.Build(ctx, options.HasFlag(MappingBuildingOptions.MarkAsReusable));
    }

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, params object[] messageArgs) =>
        base.ReportDiagnostic(descriptor, UserSymbol, messageArgs);

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

        if (SymbolAccessor.HasAccessibleParameterlessConstructor(targetType))
            return NullFallbackValue.CreateInstance;

        ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, targetType);
        return NullFallbackValue.ThrowArgumentNullException;
    }

    protected virtual MappingBuilderContext ContextForMapping(
        IMethodSymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options
    )
    {
        return new(this, userSymbol, sourceType, targetType, options.HasFlag(MappingBuildingOptions.ClearDerivedTypes));
    }

    protected ITypeMapping? BuildMapping(
        IMethodSymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options
    )
    {
        var ctx = ContextForMapping(userSymbol, sourceType.UpgradeNullable(), targetType.UpgradeNullable(), options);
        return MappingBuilder.Build(ctx, options.HasFlag(MappingBuildingOptions.MarkAsReusable));
    }
}
