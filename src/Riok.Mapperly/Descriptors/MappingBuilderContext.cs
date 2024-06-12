using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.FormatProviders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

[DebuggerDisplay("{GetType().Name}({Source} => {Target})")]
public class MappingBuilderContext : SimpleMappingBuilderContext
{
    private readonly FormatProviderCollection _formatProviders;
    private CollectionInfos? _collectionInfos;
    private DictionaryInfos? _dictionaryInfos;

    public MappingBuilderContext(
        SimpleMappingBuilderContext parentCtx,
        ObjectFactoryCollection objectFactories,
        FormatProviderCollection formatProviders,
        IUserMapping? userMapping,
        TypeMappingKey mappingKey,
        Location? diagnosticLocation = null
    )
        : base(parentCtx, diagnosticLocation ?? userMapping?.Method.GetSyntaxLocation())
    {
        ObjectFactories = objectFactories;
        _formatProviders = formatProviders;
        UserMapping = userMapping;
        MappingKey = mappingKey;
        Configuration = ReadConfiguration(new MappingConfigurationReference(UserSymbol, mappingKey.Source, mappingKey.Target));
    }

    protected MappingBuilderContext(
        MappingBuilderContext ctx,
        IUserMapping? userMapping,
        Location? diagnosticLocation,
        TypeMappingKey mappingKey,
        bool ignoreDerivedTypes
    )
        : this(ctx, ctx.ObjectFactories, ctx._formatProviders, userMapping, mappingKey, diagnosticLocation)
    {
        if (ignoreDerivedTypes)
        {
            Configuration = Configuration with { DerivedTypes = [] };
        }
    }

    public TypeMappingKey MappingKey { get; }

    public ITypeSymbol Source => MappingKey.Source;

    public ITypeSymbol Target => MappingKey.Target;

    public CollectionInfos? CollectionInfos => _collectionInfos ??= CollectionInfoBuilder.Build(Types, SymbolAccessor, Source, Target);

    public DictionaryInfos? DictionaryInfos => _dictionaryInfos ??= DictionaryInfoBuilder.Build(Types, CollectionInfos);

    public IUserMapping? UserMapping { get; }

    public IMethodSymbol? UserSymbol => UserMapping?.Method;

    public bool HasUserSymbol => UserSymbol != null;

    /// <summary>
    /// Whether the current mapping code is generated for a <see cref="System.Linq.Expressions.Expression"/>.
    /// </summary>
    public virtual bool IsExpression => false;

    public ObjectFactoryCollection ObjectFactories { get; }

    /// <inheritdoc cref="MappingBuilders.MappingBuilder.UserMappings"/>
    public IReadOnlyCollection<IUserMapping> UserMappings => MappingBuilder.UserMappings;

    /// <inheritdoc cref="MappingBuilders.MappingBuilder.NewInstanceMappings"/>
    public IReadOnlyDictionary<TypeMappingKey, INewInstanceMapping> NewInstanceMappings => MappingBuilder.NewInstanceMappings;

    /// <summary>
    /// Tries to find an existing mapping with the provided name.
    /// If none is found, <c>null</c> is returned.
    /// </summary>
    /// <param name="mappingName">The name of the mapping.</param>
    /// <returns>The found mapping, or <c>null</c> if none is found.</returns>
    public virtual INewInstanceMapping? FindNamedMapping(string mappingName)
    {
        var mapping = MappingBuilder.FindOrResolveNamed(this, mappingName, out var ambiguousName);
        if (ambiguousName)
        {
            ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingAmbiguous, mappingName);
        }

        return mapping;
    }

    /// <summary>
    /// Tries to find an existing mapping for the provided key.
    /// If none is found, <c>null</c> is returned.
    /// </summary>
    /// <param name="source">The source type</param>
    /// <param name="target">The target type</param>
    /// <returns>The found mapping, or <c>null</c> if none is found.</returns>
    public INewInstanceMapping? FindMapping(ITypeSymbol source, ITypeSymbol target) => FindMapping(new TypeMappingKey(source, target));

    /// <summary>
    /// Tries to find an existing mapping for the provided key.
    /// If none is found, <c>null</c> is returned.
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <returns>The found mapping, or <c>null</c> if none is found.</returns>
    public virtual INewInstanceMapping? FindMapping(TypeMappingKey mappingKey) => MappingBuilder.Find(mappingKey);

    /// <summary>
    /// Tries to find an existing mapping for the provided types.
    /// If none is found, a new one is created.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further calls to this method.
    /// No user symbol is passed.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="options">The mapping building options.</param>
    /// <param name="diagnosticLocation">The updated to location where to report diagnostics if a new mapping is being built.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public INewInstanceMapping? FindOrBuildMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        MappingBuildingOptions options = MappingBuildingOptions.Default,
        Location? diagnosticLocation = null
    )
    {
        return FindOrBuildMapping(new TypeMappingKey(sourceType, targetType), options, diagnosticLocation);
    }

    /// <summary>
    /// Tries to find an existing mapping for the provided mapping key.
    /// If none is found, a new one is created.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further calls to this method.
    /// No user symbol is passed.
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <param name="options">The mapping building options.</param>
    /// <param name="diagnosticLocation">The updated to location where to report diagnostics if a new mapping is being built.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public INewInstanceMapping? FindOrBuildMapping(
        TypeMappingKey mappingKey,
        MappingBuildingOptions options = MappingBuildingOptions.Default,
        Location? diagnosticLocation = null
    )
    {
        return FindMapping(mappingKey) ?? BuildMapping(mappingKey, options, diagnosticLocation);
    }

    /// <summary>
    /// Finds or builds a mapping (<seealso cref="FindOrBuildMapping(Riok.Mapperly.Descriptors.TypeMappingKey,Riok.Mapperly.Descriptors.MappingBuildingOptions,Location)"/>).
    /// Before a new mapping is built existing mappings are tried to be found by the following priorities:
    /// 1. exact match
    /// 2. ignoring the nullability of the source and the target (needs to be handled by the caller of this method)
    /// If no mapping can be found a new mapping is built with the source and the target as non-nullables.
    /// </summary>
    /// <param name="key">The mapping key.</param>
    /// <param name="options">The options to build a new mapping if no existing mapping is found.</param>
    /// <param name="diagnosticLocation">The updated to location where to report diagnostics if a new mapping is being built.</param>
    /// <returns>The found or built mapping, or <c>null</c> if none could be found and none could be built.</returns>
    public INewInstanceMapping? FindOrBuildLooseNullableMapping(
        TypeMappingKey key,
        MappingBuildingOptions options = MappingBuildingOptions.Default,
        Location? diagnosticLocation = null
    )
    {
        if (FindMapping(key) is { } mapping)
            return mapping;

        // if a user mapping is referenced
        // build it with the exact types
        // as it may expect a nullable source type
        if (key.Configuration.UseNamedMapping != null)
            return BuildMapping(key, options, diagnosticLocation);

        return FindOrBuildMapping(key.NonNullable(), options, diagnosticLocation);
    }

    /// <summary>
    /// Builds a new mapping for the provided types and config with the given options.
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <param name="options">The options.</param>
    /// <param name="diagnosticLocation">The updated to location where to report diagnostics if a new mapping is being built.</param>
    /// <returns>The created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual INewInstanceMapping? BuildMapping(
        TypeMappingKey mappingKey,
        MappingBuildingOptions options = MappingBuildingOptions.Default,
        Location? diagnosticLocation = null
    )
    {
        var userMapping = options.HasFlag(MappingBuildingOptions.KeepUserSymbol) ? UserMapping : null;
        return BuildMapping(userMapping, mappingKey, options, diagnosticLocation);
    }

    /// <summary>
    /// Tries to find an existing mapping which can work with an existing target object instance for the provided types.
    /// If none is found, a new one is created.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further calls to this method.
    /// No configuration / user symbol is passed.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    /// <param name="options">The options.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public IExistingTargetMapping? FindOrBuildExistingTargetMapping(
        ITypeSymbol source,
        ITypeSymbol target,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    ) => FindOrBuildExistingTargetMapping(new TypeMappingKey(source, target), options);

    /// <summary>
    /// Tries to find an existing mapping which can work with an existing target object instance for the provided types.
    /// If none is found, a new one is created.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further calls to this method.
    /// No configuration / user symbol is passed.
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <param name="options">The options.</param>
    /// <returns>The found or created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual IExistingTargetMapping? FindOrBuildExistingTargetMapping(
        TypeMappingKey mappingKey,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    )
    {
        return ExistingTargetMappingBuilder.Find(mappingKey) ?? BuildExistingTargetMapping(mappingKey, options);
    }

    /// <summary>
    /// Tries to build an existing target instance mapping.
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further calls to this method.
    /// No configuration / user symbol is passed.
    /// </summary>
    /// <param name="mappingKey">The mapping key.</param>
    /// <param name="options">The options.</param>
    /// <returns>The created mapping, or <c>null</c> if no mapping could be created.</returns>
    public virtual IExistingTargetMapping? BuildExistingTargetMapping(
        TypeMappingKey mappingKey,
        MappingBuildingOptions options = MappingBuildingOptions.Default
    )
    {
        var userMapping = options.HasFlag(MappingBuildingOptions.KeepUserSymbol) ? UserMapping : null;
        var ctx = ContextForMapping(userMapping, mappingKey, options);
        return ExistingTargetMappingBuilder.Build(ctx, options.HasFlag(MappingBuildingOptions.MarkAsReusable));
    }

    /// <summary>
    /// Tries to build a mapping which delegates the actual mapping to an existing mapping.
    /// Returns <c>null</c> if for the given types no mapping exists
    /// or both types (<paramref name="source"/> and <paramref name="target"/>
    /// equal the types of the context (<see cref="Source"/> and <see cref="Target"/>)
    /// (since this does not make sense (delegating to yourself) and would lead to mapping recursion).
    /// </summary>
    /// <remarks>
    /// This can be used to reuse mappings of more generalized types.
    /// E.g. a mapping from <c>List&lt;A&gt;</c> to <c>IReadOnlyCollection&lt;B&gt;</c>
    /// and a mapping from <c>IReadOnlyCollection&lt;A&gt;</c> to <c>IReadOnlyList&lt;B&gt;</c>
    /// can both use the same mapping method with a signature of <c>List&lt;B&gt; Map(IReadOnlyCollection&lt;A&gt; source)</c>.
    /// </remarks>
    /// <param name="source">The source type. <see cref="Source"/> needs to be assignable to this type.</param>
    /// <param name="target">The target type. Needs to be assignable to <see cref="Target"/>.</param>
    /// <returns>The built <see cref="DelegateMapping"/> or <c>null</c>.</returns>
    public DelegateMapping? BuildDelegatedMapping(ITypeSymbol source, ITypeSymbol target)
    {
        // don't create a delegate mapping for the same types as the current context types
        // since this would lead to a mapping recursion.
        if (
            SymbolEqualityComparer.IncludeNullability.Equals(Source, source)
            && SymbolEqualityComparer.IncludeNullability.Equals(Target, target)
        )
        {
            return null;
        }

        var existingMapping = FindMapping(source, target);
        return existingMapping == null ? null : new DelegateMapping(Source, Target, existingMapping);
    }

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, params object[] messageArgs) =>
        base.ReportDiagnostic(descriptor, null, messageArgs);

    public NullFallbackValue GetNullFallbackValue(ITypeSymbol? targetType = null) =>
        GetNullFallbackValue(targetType ?? Target, Configuration.Mapper.ThrowOnMappingNullMismatch);

    public (FormatProvider? formatProvider, bool isDefault) GetFormatProvider(string? formatProviderName)
    {
        var (formatProvider, isDefault) = _formatProviders.Get(formatProviderName);
        if (formatProviderName != null && formatProvider == null)
        {
            ReportDiagnostic(DiagnosticDescriptors.FormatProviderNotFound, formatProviderName);
        }

        return (formatProvider, isDefault);
    }

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

        if (SymbolAccessor.HasDirectlyAccessibleParameterlessConstructor(targetType))
            return NullFallbackValue.CreateInstance;

        ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, targetType);
        return NullFallbackValue.ThrowArgumentNullException;
    }

    protected virtual MappingBuilderContext ContextForMapping(
        IUserMapping? userMapping,
        TypeMappingKey mappingKey,
        MappingBuildingOptions options,
        Location? diagnosticLocation = null
    )
    {
        return new(this, userMapping, diagnosticLocation, mappingKey, options.HasFlag(MappingBuildingOptions.IgnoreDerivedTypes));
    }

    protected virtual INewInstanceMapping? BuildMapping(
        IUserMapping? userMapping,
        TypeMappingKey mappingKey,
        MappingBuildingOptions options,
        Location? diagnosticLocation
    )
    {
        var ctx = ContextForMapping(userMapping, mappingKey, options, diagnosticLocation);
        return MappingBuilder.Build(ctx, options.HasFlag(MappingBuildingOptions.MarkAsReusable));
    }
}
