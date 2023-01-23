using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MappingBuilderContext : SimpleMappingBuilderContext
{
    private readonly ISymbol? _userSymbol;

    public MappingBuilderContext(
        DescriptorBuilder builder,
        ITypeSymbol source,
        ITypeSymbol target,
        ISymbol? userSymbol)
        : base(builder)
    {
        Source = source;
        Target = target;
        _userSymbol = userSymbol;
    }

    public ITypeSymbol Source { get; }

    public ITypeSymbol Target { get; }

    public ObjectFactoryCollection ObjectFactories => Builder.ObjectFactories;

    /// <summary>
    /// Tries to find an existing mapping for the provided types.
    /// If none is found, <c>null</c> is returned.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The found mapping, or <c>null</c> if none is found.</returns>
    public ITypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => Builder.MappingBuilder.FindMapping(sourceType.UpgradeNullable(), targetType.UpgradeNullable());

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
    public ITypeMapping? FindOrBuildMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => Builder.MappingBuilder.FindOrBuild(sourceType.UpgradeNullable(), targetType.UpgradeNullable());

    /// <summary>
    /// Tries to build a new mapping for the given types.
    /// The built mapping is not added to the mapping descriptor (should only be used as a delegate to another mapping)
    /// and is therefore not accessible by other mappings.
    /// Configuration / the user symbol is passed from the caller.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    /// <returns>The created mapping or <c>null</c> if none could be created.</returns>
    public ITypeMapping? BuildDelegateMapping(ITypeSymbol source, ITypeSymbol target)
        => Builder.MappingBuilder.BuildDelegate(_userSymbol, source.UpgradeNullable(), target.UpgradeNullable());

    /// <summary>
    /// Tries to build a new mapping for the given types while keeping the current user symbol reference.
    /// This reuses configurations on the user symbol for the to be built mapping (eg. <see cref="Riok.Mapperly.Abstractions.MapPropertyAttribute"/>).
    /// If no mapping is possible for the provided types,
    /// <c>null</c> is returned.
    /// If a new mapping is created, it is added to the mapping descriptor
    /// and returned in further <see cref="FindOrBuildMapping"/> calls.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ITypeMapping? BuildMappingWithUserSymbol(ITypeSymbol source, ITypeSymbol target)
        => Builder.MappingBuilder.BuildWithUserSymbol(
            _userSymbol ?? throw new InvalidOperationException(nameof(BuildMappingWithUserSymbol) + " can only be called for contexts with a user symbol"),
            source.UpgradeNullable(),
            target.UpgradeNullable());

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
    public IExistingTargetMapping? FindOrBuildExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => Builder.ExistingTargetMappingBuilder.FindOrBuild(_userSymbol, sourceType, targetType);

    public T GetConfigurationOrDefault<T>() where T : Attribute
    {
        return ListConfiguration<T>().FirstOrDefault()
            ?? (T)Builder.DefaultConfigurations[typeof(T)];
    }

    public IEnumerable<T> ListConfiguration<T>() where T : Attribute
    {
        return _userSymbol == null
            ? Enumerable.Empty<T>()
            : AttributeDataAccessor.Access<T>(Compilation, _userSymbol);
    }

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, params object[] messageArgs)
        => base.ReportDiagnostic(descriptor, _userSymbol, messageArgs);

    public NullFallbackValue GetNullFallbackValue(ITypeSymbol? targetType = null)
    {
        targetType ??= Target;
        if (targetType.IsNullable())
            return NullFallbackValue.Default;

        if (MapperConfiguration.ThrowOnMappingNullMismatch)
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
}
