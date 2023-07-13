using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class SymbolAccessor
{
    private readonly WellKnownTypes _types;
    private readonly Dictionary<ISymbol, ImmutableArray<AttributeData>> _attributes = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyCollection<ISymbol>> _allMembers = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyCollection<IMappableMember>> _allAccessibleMembers =
        new(SymbolEqualityComparer.Default);

    public SymbolAccessor(WellKnownTypes types)
    {
        _types = types;
    }

    internal IEnumerable<AttributeData> GetAttributes<T>(ISymbol symbol)
        where T : Attribute
    {
        var attributes = GetAttributesCore(symbol);
        if (attributes.IsEmpty)
        {
            yield break;
        }

        var attributeSymbol = _types.Get<T>();
        foreach (var attr in attributes)
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass?.ConstructedFrom ?? attr.AttributeClass, attributeSymbol))
            {
                yield return attr;
            }
        }
    }

    internal bool HasAttribute<T>(ISymbol symbol)
        where T : Attribute => GetAttributes<T>(symbol).Any();

    internal IEnumerable<IMethodSymbol> GetAllMethods(ITypeSymbol symbol) => GetAllMembers(symbol).OfType<IMethodSymbol>();

    internal IEnumerable<IMethodSymbol> GetAllMethods(ITypeSymbol symbol, string name) =>
        GetAllMembers(symbol, name).OfType<IMethodSymbol>();

    internal IEnumerable<IPropertySymbol> GetAllProperties(ITypeSymbol symbol, string name) =>
        GetAllMembers(symbol, name).OfType<IPropertySymbol>();

    internal IEnumerable<IFieldSymbol> GetAllFields(ITypeSymbol symbol) => GetAllMembers(symbol).OfType<IFieldSymbol>();

    internal IReadOnlyCollection<ISymbol> GetAllMembers(ITypeSymbol symbol)
    {
        if (_allMembers.TryGetValue(symbol, out var members))
        {
            return members;
        }

        members = GetAllMembersCore(symbol).ToArray();
        _allMembers.Add(symbol, members);

        return members;
    }

    internal IReadOnlyCollection<IMappableMember> GetAllAccessibleMappableMembers(ITypeSymbol symbol)
    {
        if (_allAccessibleMembers.TryGetValue(symbol, out var members))
        {
            return members;
        }

        members = GetAllAccessibleMappableMembersCore(symbol).ToArray();
        _allAccessibleMembers.Add(symbol, members);

        return members;
    }

    internal IEnumerable<IMappableMember> GetMappableMembers(ITypeSymbol symbol, string name, IEqualityComparer<string> comparer)
    {
        foreach (var member in GetAllAccessibleMappableMembers(symbol))
        {
            if (comparer.Equals(name, member.Name))
            {
                yield return member;
            }
        }
    }

    private IEnumerable<ISymbol> GetAllMembers(ITypeSymbol symbol, string name) => GetAllMembers(symbol).Where(x => name.Equals(x.Name));

    private ImmutableArray<AttributeData> GetAttributesCore(ISymbol symbol)
    {
        if (_attributes.TryGetValue(symbol, out var attributes))
        {
            return attributes;
        }

        attributes = symbol.GetAttributes();
        _attributes.Add(symbol, attributes);

        return attributes;
    }

    private IEnumerable<ISymbol> GetAllMembersCore(ITypeSymbol symbol)
    {
        var members = symbol.GetMembers();

        if (symbol.TypeKind == TypeKind.Interface)
        {
            var interfaceProperties = symbol.AllInterfaces.SelectMany(GetAllMembers);
            return members.Concat(interfaceProperties);
        }

        return symbol.BaseType == null ? members : members.Concat(GetAllMembers(symbol.BaseType));
    }

    private IEnumerable<IMappableMember> GetAllAccessibleMappableMembersCore(ITypeSymbol symbol)
    {
        return GetAllMembers(symbol)
            .Where(x => x is { IsStatic: false, Kind: SymbolKind.Property or SymbolKind.Field } && x.IsAccessible())
            .DistinctBy(x => x.Name)
            .Select(MappableMember.Create)
            .WhereNotNull();
    }
}
