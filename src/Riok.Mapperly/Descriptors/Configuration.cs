using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;

namespace Riok.Mapperly.Descriptors;

public class Configuration
{
    /// <summary>
    /// Default configurations, used if a configuration is required for a mapping
    /// but no configuration is provided by the user.
    /// These are the default configurations registered for each configuration attribute (eg. the <see cref="MapEnumAttribute"/>).
    /// Usually these are derived from the <see cref="MapperAttribute"/> or default values.
    /// </summary>
    private readonly Dictionary<Type, object> _defaultConfigurations = new();

    private readonly Compilation _compilation;

    public Configuration(Compilation compilation, INamedTypeSymbol mapperSymbol)
    {
        _compilation = compilation;
        Mapper = AttributeDataAccessor.AccessFirstOrDefault<MapperAttribute>(compilation, mapperSymbol) ?? new();
        InitDefaultConfigurations();
    }

    public MapperAttribute Mapper { get; }

    public T GetOrDefault<T>(IMethodSymbol? userSymbol)
        where T : Attribute
    {
        return ListConfiguration<T, T>(userSymbol).FirstOrDefault() ?? (T)_defaultConfigurations[typeof(T)];
    }

    public IEnumerable<TData> ListConfiguration<T, TData>(IMethodSymbol? userSymbol)
        where T : Attribute
    {
        return userSymbol == null ? Enumerable.Empty<TData>() : AttributeDataAccessor.Access<T, TData>(_compilation, userSymbol);
    }

    private void InitDefaultConfigurations()
    {
        _defaultConfigurations.Add(
            typeof(MapEnumAttribute),
            new MapEnumAttribute(Mapper.EnumMappingStrategy) { IgnoreCase = Mapper.EnumMappingIgnoreCase }
        );
    }
}
