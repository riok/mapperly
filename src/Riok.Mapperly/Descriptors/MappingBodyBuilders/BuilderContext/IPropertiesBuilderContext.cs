using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// Context to build property mappings.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public interface IPropertiesBuilderContext<out T>
    where T : IMapping
{
    T Mapping { get; }

    void AddDiagnostics();

    MappingBuilderContext BuilderContext { get; }

    IReadOnlyCollection<string> IgnoredSourcePropertyNames { get; }

    Dictionary<string, IPropertySymbol> TargetProperties { get; }

    Dictionary<string, List<MapPropertyAttribute>> PropertyConfigsByRootTargetName { get; }
}
