using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

public class UnsafeAccessorContext(UniqueNameBuilder nameBuilder, SymbolAccessor symbolAccessor)
{
    private readonly List<IUnsafeAccessor> _unsafeAccessors = new();
    private readonly Dictionary<UnsafeAccessorKey, IUnsafeAccessor> _unsafeAccessorsBySymbol = new();
    private readonly UniqueNameBuilder _nameBuilder = nameBuilder.NewScope();
    public IReadOnlyCollection<IUnsafeAccessor> UnsafeAccessors => _unsafeAccessors;

    public UnsafeSetPropertyAccessor GetOrBuildPropertySetter(PropertyMember member)
    {
        return GetOrBuild(
            "Set",
            UnsafeAccessorType.SetProperty,
            member.Symbol,
            static (m, methodName) => new UnsafeSetPropertyAccessor(m, methodName)
        );
    }

    public UnsafeGetPropertyAccessor GetOrBuildPropertyGetter(PropertyMember member)
    {
        return GetOrBuild(
            "Get",
            UnsafeAccessorType.GetProperty,
            member.Symbol,
            static (m, methodName) => new UnsafeGetPropertyAccessor(m, methodName)
        );
    }

    public UnsafeFieldAccessor GetOrBuildFieldGetter(FieldMember member)
    {
        return GetOrBuild(
            "Get",
            UnsafeAccessorType.GetField,
            member.Symbol,
            static (m, methodName) => new UnsafeFieldAccessor(m, methodName)
        );
    }

    private TAccessor GetOrBuild<TAccessor, TSymbol>(
        string methodNamePrefix,
        UnsafeAccessorType type,
        TSymbol symbol,
        Func<TSymbol, string, TAccessor> factory
    )
        where TAccessor : IUnsafeAccessor
        where TSymbol : ISymbol
    {
        var key = new UnsafeAccessorKey(symbol, type);
        if (TryGetAccessor<TAccessor>(key, out var accessor))
            return accessor;

        var methodName = BuildMethodName(methodNamePrefix, symbol);
        return CacheAccessor(key, factory(symbol, methodName));
    }

    private T CacheAccessor<T>(UnsafeAccessorKey key, T accessor)
        where T : IUnsafeAccessor
    {
        _unsafeAccessorsBySymbol.Add(key, accessor);
        _unsafeAccessors.Add(accessor);
        return accessor;
    }

    private bool TryGetAccessor<T>(UnsafeAccessorKey key, [NotNullWhen(true)] out T? accessor)
        where T : IUnsafeAccessor
    {
        if (_unsafeAccessorsBySymbol.TryGetValue(key, out var acc))
        {
            accessor = (T)acc;
            return true;
        }

        accessor = default;
        return false;
    }

    private string BuildMethodName(string prefix, ISymbol symbol)
    {
        var methodName = prefix + FormatAccessorName(symbol.Name);
        return GetUniqueMethodName(symbol.ContainingType, methodName);
    }

    private string GetUniqueMethodName(ITypeSymbol symbol, string name)
    {
        var memberNames = symbolAccessor.GetAllMembers(symbol).Select(x => x.Name);
        return _nameBuilder.New(name, memberNames);
    }

    /// <summary>
    /// Strips the leading underscore and capitalise the first letter.
    /// </summary>
    /// <param name="name">Accessor name to be formatted.</param>
    /// <returns>Formatted accessor name.</returns>
    private string FormatAccessorName(string name)
    {
        name = name.TrimStart('_');
        if (name.Length == 0)
            return name;

        return char.ToUpper(name[0], CultureInfo.InvariantCulture) + name[1..];
    }

    private enum UnsafeAccessorType
    {
        GetProperty,
        SetProperty,
        GetField,
    }

    private readonly struct UnsafeAccessorKey(ISymbol member, UnsafeAccessorType type) : IEquatable<UnsafeAccessorKey>
    {
        private readonly ISymbol _member = member;
        private readonly UnsafeAccessorType _type = type;

        public bool Equals(UnsafeAccessorKey other) =>
            _type == other._type && SymbolEqualityComparer.Default.Equals(_member, other._member);

        public override bool Equals(object? obj) => obj is UnsafeAccessorKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SymbolEqualityComparer.Default.GetHashCode(_member);
                hashCode = (hashCode * 397) ^ (int)_type;
                return hashCode;
            }
        }
    }
}
