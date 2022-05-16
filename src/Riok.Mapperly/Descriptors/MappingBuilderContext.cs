using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MappingBuilderContext : SimpleMappingBuilderContext
{
    private readonly DescriptorBuilder _builder;
    private readonly ISymbol? _userSymbol;

    public MappingBuilderContext(
        DescriptorBuilder builder,
        ITypeSymbol source,
        ITypeSymbol target,
        ISymbol? userSymbol)
        : base(builder)
    {
        _builder = builder;
        Source = source;
        Target = target;
        _userSymbol = userSymbol;
    }

    public ITypeSymbol Source { get; }

    public ITypeSymbol Target { get; }

    public INamedTypeSymbol GetTypeSymbol(Type type)
        => Compilation.GetTypeByMetadataName(type.FullName ?? throw new InvalidOperationException("Could not get name of type " + type))
            ?? throw new InvalidOperationException("Could not get type " + type.FullName);

    public bool IsType(ITypeSymbol symbol, Type type)
        => SymbolEqualityComparer.Default.Equals(symbol, GetTypeSymbol(type));

    public TypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => _builder.FindMapping(sourceType.UpgradeNullable(), targetType.UpgradeNullable());

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
    public TypeMapping? FindOrBuildMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => _builder.FindOrBuildMapping(sourceType.UpgradeNullable(), targetType.UpgradeNullable());

    /// <summary>
    /// Tries to build a new mapping for the given types.
    /// The built mapping is not added to the mapping descriptor (should only be used as a delegate to another mapping)
    /// and is therefore not accessible by other mappings.
    /// Configuration / the user symbol is passed from the caller.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    /// <returns>The created mapping or <c>null</c> if none could be created.</returns>
    public TypeMapping? BuildDelegateMapping(ITypeSymbol source, ITypeSymbol target)
        => _builder.BuildDelegateMapping(_userSymbol, source.UpgradeNullable(), target.UpgradeNullable());

    public T GetConfigurationOrDefault<T>() where T : Attribute
    {
        return ListConfiguration<T>().FirstOrDefault()
            ?? (T)_builder.DefaultConfigurations[typeof(T)];
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
