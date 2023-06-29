using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class WellKnownTypes
{
    private readonly Compilation _compilation;
    private readonly Dictionary<string, INamedTypeSymbol?> _cachedTypes = new();
    private readonly Dictionary<ISymbol, ImmutableArray<AttributeData>> _cachedAttributes = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, ISymbol[]> _allMembers = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IMappableMember[]> _allAccessibleMembers = new(SymbolEqualityComparer.Default);

    internal WellKnownTypes(Compilation compilation)
    {
        _compilation = compilation;
    }

    // use string type name as they are not available in netstandard2.0
    public INamedTypeSymbol? DateOnly => TryGet("System.DateOnly");

    public INamedTypeSymbol? TimeOnly => TryGet("System.TimeOnly");

    public INamedTypeSymbol Get<T>() => Get(typeof(T).FullName);

    public INamedTypeSymbol Get(Type type) =>
        Get(type.FullName ?? throw new InvalidOperationException("Could not get name of type " + type));

    public INamedTypeSymbol Get(string typeFullName) =>
        TryGet(typeFullName) ?? throw new InvalidOperationException("Could not get type " + typeFullName);

    public INamedTypeSymbol? TryGet(string typeFullName)
    {
        if (_cachedTypes.TryGetValue(typeFullName, out var typeSymbol))
        {
            return typeSymbol;
        }

        typeSymbol = _compilation.GetTypeByMetadataName(typeFullName);
        _cachedTypes.Add(typeFullName, typeSymbol);

        return typeSymbol;
    }

    public ImmutableArray<AttributeData> GetAttributes(ISymbol symbol)
    {
        if (_cachedAttributes.TryGetValue(symbol, out var attributes))
        {
            return attributes;
        }

        attributes = symbol.GetAttributes();
        _cachedAttributes.Add(symbol, attributes);

        return attributes;
    }

    public ISymbol[] GetAllMembers(ITypeSymbol symbol)
    {
        if (_allMembers.TryGetValue(symbol, out var members))
        {
            return members;
        }

        members = GetAllMembersCore(symbol).ToArray();
        _allMembers.Add(symbol, members);

        return members;
    }

    public IMappableMember[] GetAllAccessibleMappableMembers(ITypeSymbol symbol)
    {
        if (_allAccessibleMembers.TryGetValue(symbol, out var members))
        {
            return members;
        }

        members = GetAllAccessibleMappableMembersCore(symbol);
        _allAccessibleMembers.Add(symbol, members);

        return members;
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

    private IMappableMember[] GetAllAccessibleMappableMembersCore(ITypeSymbol symbol)
    {
        return GetAllMembers(symbol)
            .Where(x => !x.IsStatic && x.IsAccessible() && x.Kind is SymbolKind.Property or SymbolKind.Field)
            .DistinctBy(x => x.Name)
            .Select(MappableMember.Create)
            .WhereNotNull()
            .ToArray();
    }
}
