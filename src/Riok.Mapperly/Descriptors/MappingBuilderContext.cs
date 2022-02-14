using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.TypeMappings;

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
        => _builder.FindOrBuildMapping(sourceType, targetType);

    /// <summary>
    /// Tries to build a new mapping for the given types.
    /// The built mapping is not added to the mapping descriptor.
    /// Configuration / the user symbol is passed from the caller.
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    /// <returns>The created mapping or <c>null</c> if none could be created.</returns>
    public TypeMapping? TryBuildNewMapping(ITypeSymbol source, ITypeSymbol target)
        => _builder.TryBuildNewMapping(_userSymbol, source, target);

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
}
