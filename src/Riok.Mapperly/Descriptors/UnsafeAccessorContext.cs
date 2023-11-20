using System.Globalization;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings.UnsafeAccess;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class UnsafeAccessorContext(UniqueNameBuilder nameBuilder, SymbolAccessor symbolAccessor)
{
    private readonly UniqueNameBuilder _nameBuilder = nameBuilder.NewScope();
    private readonly Dictionary<UnsafeAccessorKey, IUnsafeAccessor> _unsafeAccessors = new();

    public IReadOnlyCollection<IUnsafeAccessor> UnsafeAccessors => _unsafeAccessors.Values;

    public IUnsafeAccessor GetOrBuildAccessor(UnsafeAccessorType type, ISymbol symbol)
    {
        var key = new UnsafeAccessorKey(symbol, type);
        if (_unsafeAccessors.TryGetValue(key, out var value))
            return value;

        var formatted = FormatAccessorName(symbol.Name);

        var defaultMethodName = type switch
        {
            UnsafeAccessorType.GetField => $"Get{formatted}",
            UnsafeAccessorType.GetProperty => $"Get{formatted}",
            UnsafeAccessorType.SetProperty => $"Set{formatted}",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(UnsafeAccessorType)}"),
        };
        var methodName = GetValidMethodName(symbol.ContainingType, defaultMethodName);

        IUnsafeAccessor accessor = type switch
        {
            UnsafeAccessorType.GetField => new UnsafeFieldAccessor((IFieldSymbol)symbol, methodName),
            UnsafeAccessorType.GetProperty => new UnsafeGetPropertyAccessor((IPropertySymbol)symbol, methodName),
            UnsafeAccessorType.SetProperty => new UnsafeSetPropertyAccessor((IPropertySymbol)symbol, methodName),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(UnsafeAccessorType)}"),
        };

        _unsafeAccessors.Add(key, accessor);
        return accessor;
    }

    private string GetValidMethodName(ITypeSymbol symbol, string name)
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

    public enum UnsafeAccessorType
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
            SymbolEqualityComparer.Default.Equals(_member, other._member) && _type == other._type;

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
