using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public class CachedAttributeDataAccessor(IAttributeDataAccessor attributeDataAccessor) : IAttributeDataAccessor
{
    private readonly Dictionary<CacheKey, object?> _cache = new();
    private readonly IAttributeDataAccessor _attributeDataAccessor = attributeDataAccessor;

    public FormatProviderAttribute ReadFormatProviderAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadFormatProviderAttribute(s))
            ?? throw new InvalidOperationException($"Could not find attribute {typeof(FormatProviderAttribute).FullName} on {symbol}");
    }

    public MapperConfiguration ReadMapperAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadMapperAttribute(s))
            ?? throw new InvalidOperationException($"Could not find attribute {typeof(MapperAttribute).FullName} on {symbol}");
    }

    public MapperIgnoreObsoleteMembersAttribute? ReadMapperIgnoreObsoleteMembersAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadMapperIgnoreObsoleteMembersAttribute(s));
    }

    public IEnumerable<NestedMembersMappingConfiguration> ReadMapNestedPropertiesAttribute(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapNestedPropertiesAttribute(s));
    }

    public MapperRequiredMappingAttribute? ReadMapperRequiredMappingAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadMapperRequiredMappingAttribute(s));
    }

    public EnumMemberAttribute? ReadEnumMemberAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadEnumMemberAttribute(s));
    }

    public EnumConfiguration? ReadMapEnumAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadMapEnumAttribute(s));
    }

    public IEnumerable<EnumValueMappingConfiguration> ReadMapEnumValueAttribute(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapEnumValueAttribute(s));
    }

    public IEnumerable<MapperIgnoreEnumValueConfiguration> ReadMapperIgnoreSourceValueAttribute(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapperIgnoreSourceValueAttribute(s));
    }

    public IEnumerable<MapperIgnoreEnumValueConfiguration> ReadMapperIgnoreTargetValueAttribute(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapperIgnoreTargetValueAttribute(s));
    }

    public ComponentModelDescriptionAttributeConfiguration? ReadDescriptionAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadDescriptionAttribute(s));
    }

    public UserMappingConfiguration? ReadUserMappingAttribute(ISymbol symbol)
    {
        return GetOrCreateValue(symbol, (s, x) => x.ReadUserMappingAttribute(s));
    }

    public bool HasUseMapperAttribute(ISymbol symbol)
    {
        return _attributeDataAccessor.HasUseMapperAttribute(symbol);
    }

    public IEnumerable<MapperIgnoreSourceAttribute> ReadMapperIgnoreSourceAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapperIgnoreSourceAttributes(s));
    }

    public IEnumerable<MapperIgnoreTargetAttribute> ReadMapperIgnoreTargetAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapperIgnoreTargetAttributes(s));
    }

    public IEnumerable<MemberValueMappingConfiguration> ReadMapValueAttribute(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapValueAttribute(s));
    }

    public IEnumerable<MemberMappingConfiguration> ReadMapPropertyAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapPropertyAttributes(s));
    }

    public IEnumerable<IncludeMappingConfiguration> ReadIncludeMappingConfigurationAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadIncludeMappingConfigurationAttributes(s));
    }

    public IEnumerable<DerivedTypeMappingConfiguration> ReadMapDerivedTypeAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapDerivedTypeAttributes(s));
    }

    public IEnumerable<DerivedTypeMappingConfiguration> ReadGenericMapDerivedTypeAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadGenericMapDerivedTypeAttributes(s));
    }

    public IEnumerable<MemberMappingConfiguration> ReadMapPropertyFromSourceAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadMapPropertyFromSourceAttributes(s));
    }

    public IEnumerable<UseStaticMapperConfiguration> ReadUseStaticMapperAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadUseStaticMapperAttributes(s));
    }

    public IEnumerable<UseStaticMapperConfiguration> ReadGenericUseStaticMapperAttributes(ISymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadGenericUseStaticMapperAttributes(s));
    }

    public string GetMappingName(IMethodSymbol methodSymbol)
    {
        return GetOrCreateValue(methodSymbol, (s, x) => x.GetMappingName((IMethodSymbol)s)) ?? methodSymbol.Name;
    }

    public bool IsMappingNameEqualTo(IMethodSymbol methodSymbol, string name)
    {
        return string.Equals(GetMappingName(methodSymbol), name, StringComparison.Ordinal);
    }

    public IEnumerable<NotNullIfNotNullConfiguration> ReadNotNullIfNotNullAttributes(IMethodSymbol symbol)
    {
        return GetOrCreateValues(symbol, (s, x) => x.ReadNotNullIfNotNullAttributes((IMethodSymbol)s));
    }

    private T? GetOrCreateValue<T>(
        ISymbol symbol,
        Func<ISymbol, IAttributeDataAccessor, T?> createValue,
        [CallerMemberName] string name = ""
    )
        where T : class
    {
        var key = new CacheKey(name, symbol);
        if (_cache.TryGetValue(key, out var value))
        {
            return value as T;
        }

        var newValue = createValue(symbol, _attributeDataAccessor);
        _cache.Add(key, newValue);
        return newValue;
    }

    private IReadOnlyList<T> GetOrCreateValues<T>(
        ISymbol symbol,
        Func<ISymbol, IAttributeDataAccessor, IEnumerable<T>> createValue,
        [CallerMemberName] string name = ""
    )
    {
        var key = new CacheKey(name, symbol);
        if (_cache.TryGetValue(key, out var value))
        {
            return (IReadOnlyList<T>)value!;
        }

        var newValue = createValue(symbol, _attributeDataAccessor);
        var enumerated = newValue.ToList().AsReadOnly();
        _cache.Add(key, enumerated);
        return enumerated;
    }

    private readonly struct CacheKey(string name, ISymbol symbol) : IEquatable<CacheKey>
    {
        private readonly string _name = name;
        private readonly ISymbol _symbol = symbol;

        public override int GetHashCode()
        {
            var nameHashCode = StringComparer.Ordinal.GetHashCode(_name);
            var symbolHashCode = SymbolEqualityComparer.Default.GetHashCode(_symbol);
            return HashCode.Combine(nameHashCode, symbolHashCode);
        }

        public bool Equals(CacheKey other)
        {
            return string.Equals(_name, other._name, StringComparison.Ordinal)
                && SymbolEqualityComparer.Default.Equals(_symbol, other._symbol);
        }

        public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);

        public static bool operator ==(CacheKey left, CacheKey right) => left.Equals(right);

        public static bool operator !=(CacheKey left, CacheKey right) => !left.Equals(right);
    }
}
