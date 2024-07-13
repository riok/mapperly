using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors;

public class SymbolAccessor(CompilationContext compilationContext, INamedTypeSymbol mapperSymbol)
{
    // this is a weak reference table
    // since if there is no reference to the key
    // the values should not be kept in the memory anymore / are not needed anymore.
    private readonly ConditionalWeakTable<ITypeSymbol, ITypeSymbol> _originalNullableTypes = new();
    private readonly Dictionary<ISymbol, ImmutableArray<AttributeData>> _attributes = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyCollection<ISymbol>> _allMembers = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyCollection<IMappableMember>> _allAccessibleMembers =
        new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyDictionary<string, IMappableMember>> _allAccessibleMembersCaseInsensitive =
        new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyDictionary<string, IMappableMember>> _allAccessibleMembersCaseSensitive =
        new(SymbolEqualityComparer.Default);

    private MemberVisibility _memberVisibility = MemberVisibility.AllAccessible;
    private MemberVisibility _constructorVisibility = MemberVisibility.AllAccessible;

    private Compilation Compilation => compilationContext.Compilation;

    internal void SetMemberVisibility(MemberVisibility visibility) => _memberVisibility = visibility;

    internal void SetConstructorVisibility(MemberVisibility visibility) => _constructorVisibility = visibility;

    public bool HasDirectlyAccessibleParameterlessConstructor(ITypeSymbol symbol) =>
        symbol is INamedTypeSymbol { IsAbstract: false } namedTypeSymbol
        && namedTypeSymbol.InstanceConstructors.Any(c => c.Parameters.IsDefaultOrEmpty && IsDirectlyAccessible(c));

    public bool HasAccessibleParameterlessConstructor(ITypeSymbol symbol) =>
        symbol is INamedTypeSymbol { IsAbstract: false } namedTypeSymbol
        && namedTypeSymbol.InstanceConstructors.Any(x => x.Parameters.IsDefaultOrEmpty && IsConstructorAccessible(x));

    public bool HasAnyAccessibleConstructor(ITypeSymbol symbol) =>
        symbol is INamedTypeSymbol { IsAbstract: false } namedTypeSymbol
        && namedTypeSymbol.InstanceConstructors.Any(IsConstructorAccessible);

    public bool IsDirectlyAccessible(ISymbol symbol) => Compilation.IsSymbolAccessibleWithin(symbol, mapperSymbol);

    public bool IsMemberAccessible(ISymbol symbol)
    {
        Debug.Assert(symbol is not IMethodSymbol { MethodKind: MethodKind.Constructor });
        return IsAccessible(symbol, _memberVisibility);
    }

    public bool IsConstructorAccessible(IMethodSymbol symbol)
    {
        Debug.Assert(symbol.MethodKind == MethodKind.Constructor);
        return IsAccessible(symbol, _constructorVisibility);
    }

    private bool IsAccessible(ISymbol symbol, MemberVisibility visibility)
    {
        if (visibility.HasFlag(MemberVisibility.Accessible) && !IsDirectlyAccessible(symbol))
            return false;

        return symbol.DeclaredAccessibility switch
        {
            Accessibility.Private => visibility.HasFlag(MemberVisibility.Private),
            Accessibility.ProtectedAndInternal
                => visibility.HasFlag(MemberVisibility.Protected) && visibility.HasFlag(MemberVisibility.Internal),
            Accessibility.Protected => visibility.HasFlag(MemberVisibility.Protected),
            Accessibility.Internal => visibility.HasFlag(MemberVisibility.Internal),
            Accessibility.ProtectedOrInternal
                => visibility.HasFlag(MemberVisibility.Protected) || visibility.HasFlag(MemberVisibility.Internal),
            Accessibility.Public => visibility.HasFlag(MemberVisibility.Public),
            _ => false,
        };
    }

    public bool HasImplicitConversion(ITypeSymbol source, ITypeSymbol destination) =>
        Compilation.ClassifyConversion(source, destination).IsImplicit && (destination.IsNullable() || !source.IsNullable());

    /// <summary>
    /// Returns true when a conversion form the <paramref name="sourceType"/>
    /// to the <paramref name="targetType"/> is possible with a conversion
    /// of type identity, boxing or implicit and compatible nullability.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>Whether the assignment is valid</returns>
    public bool CanAssign(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        var conversion = Compilation.ClassifyConversion(sourceType, targetType);
        return (conversion.IsIdentity || conversion.IsBoxing || conversion.IsImplicit)
            && (targetType.IsNullable() || !sourceType.IsNullable());
    }

    public MethodParameter? WrapOptionalMethodParameter(IParameterSymbol? symbol) => symbol == null ? null : WrapMethodParameter(symbol);

    public MethodParameter WrapMethodParameter(IParameterSymbol symbol) => new(symbol, UpgradeNullable(symbol.Type));

    /// <summary>
    /// Upgrade the nullability of a symbol from <see cref="NullableAnnotation.None"/> to <see cref="NullableAnnotation.Annotated"/>.
    /// Value types are not upgraded.
    /// </summary>
    /// <param name="symbol">The symbol to upgrade.</param>
    /// <returns>The upgraded symbol</returns>
    internal ITypeSymbol UpgradeNullable(ITypeSymbol symbol)
    {
        TryUpgradeNullable(symbol, out var upgradedSymbol);
        return upgradedSymbol ?? symbol;
    }

    /// <summary>
    /// Tries to upgrade the nullability of a symbol from <see cref="NullableAnnotation.None"/> to <see cref="NullableAnnotation.Annotated"/>.
    /// Value types are not upgraded.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="upgradedSymbol">The upgraded symbol, if an upgrade has taken place, <c>null</c> otherwise.</param>
    /// <returns>Whether an upgrade has taken place.</returns>
    internal bool TryUpgradeNullable(ITypeSymbol symbol, [NotNullWhen(true)] out ITypeSymbol? upgradedSymbol)
    {
        if (symbol.NullableAnnotation != NullableAnnotation.None || symbol.IsValueType)
        {
            upgradedSymbol = default;
            return false;
        }

        switch (symbol)
        {
            case INamedTypeSymbol { TypeArguments.Length: > 0 } namedSymbol:
                var upgradedTypeArguments = namedSymbol.TypeArguments.Select(UpgradeNullable).ToImmutableArray();
                upgradedSymbol = namedSymbol
                    .ConstructedFrom.Construct(
                        upgradedTypeArguments,
                        upgradedTypeArguments.Select(ta => ta.NullableAnnotation).ToImmutableArray()
                    )
                    .WithNullableAnnotation(NullableAnnotation.Annotated);
                break;

            case IArrayTypeSymbol { ElementType.IsValueType: false, ElementNullableAnnotation: NullableAnnotation.None } arrayTypeSymbol:
                upgradedSymbol = Compilation
                    .CreateArrayTypeSymbol(UpgradeNullable(arrayTypeSymbol.ElementType), arrayTypeSymbol.Rank, NullableAnnotation.Annotated)
                    .WithNullableAnnotation(NullableAnnotation.Annotated);
                break;

            default:
                upgradedSymbol = symbol.WithNullableAnnotation(NullableAnnotation.Annotated);
                break;
        }

        _originalNullableTypes.Add(upgradedSymbol, symbol);
        return true;
    }

    /// <summary>
    /// Returns a non-nullable variant of <paramref name="type"/>
    /// if the <paramref name="userMappingType"/> declared by the user
    /// does not have nullable annotations (<see cref="NullableAnnotation.None"/>).
    /// If no user-mapping type is provided, the <paramref name="type"/> is used
    /// to resolve the original nullable values.
    /// This can be used in contexts where the original nullable annotations are important
    /// (the not-yet "upgraded" values).
    /// Usually this is the case if <see cref="NullableAnnotation.None"/> should
    /// behave differently than <see cref="NullableAnnotation.Annotated"/>
    /// (<see cref="NullableAnnotation.None"/> is upgraded to <see cref="NullableAnnotation.Annotated"/>
    /// while reading the user symbols).
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="userMappingType">The user mapping type.</param>
    /// <returns>The <paramref name="type"/> or its non-nullable variant.</returns>
    internal ITypeSymbol NonNullableIfNullableReferenceTypesDisabled(ITypeSymbol type, ITypeSymbol? userMappingType = null)
    {
        if (
            type.IsNullableReferenceType()
            && _originalNullableTypes.TryGetValue(userMappingType ?? type, out var originalType)
            && originalType.NullableAnnotation == NullableAnnotation.None
        )
        {
            return type.NonNullable();
        }

        return type;
    }

    internal IEnumerable<AttributeData> GetAttributes<T>(ISymbol symbol)
        where T : Attribute
    {
        var attributes = GetAttributesCore(symbol);
        if (attributes.IsEmpty)
        {
            yield break;
        }

        var attributeSymbol = compilationContext.Types.Get<T>();
        foreach (var attr in attributes)
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass?.ConstructedFrom ?? attr.AttributeClass, attributeSymbol))
            {
                yield return attr;
            }
        }
    }

    internal static IEnumerable<AttributeData> GetAttributesSkipCache(ISymbol symbol, INamedTypeSymbol attributeSymbol)
    {
        foreach (var attr in symbol.GetAttributes())
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
        GetAllMembers(symbol).Where(x => string.Equals(x.Name, name, StringComparison.Ordinal)).OfType<IMethodSymbol>();

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

    internal bool TryFindMemberPath(
        IReadOnlyDictionary<string, IMappableMember> members,
        IEnumerable<IReadOnlyList<string>> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? memberPath
    )
    {
        var foundPath = new List<IMappableMember>();
        foreach (var pathCandidate in pathCandidates)
        {
            if (!members.TryGetValue(pathCandidate[0], out var member))
                continue;

            foundPath.Clear();
            foundPath.Add(member);
            if (pathCandidate.Count == 1 || TryFindPath(member.Type, pathCandidate.Skip(1), ignoreCase, foundPath))
            {
                memberPath = new NonEmptyMemberPath(member.Type, foundPath);
                return true;
            }
        }

        memberPath = null;
        return false;
    }

    internal bool TryFindMemberPath(
        ITypeSymbol type,
        IEnumerable<IReadOnlyList<string>> pathCandidates,
        IReadOnlyCollection<string> ignoredNames,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? memberPath
    )
    {
        var foundPath = new List<IMappableMember>();
        foreach (var pathCandidate in pathCandidates)
        {
            // fast path for exact case matches
            if (ignoredNames.Contains(pathCandidate[0]))
                continue;

            // reuse List instead of allocating a new one
            foundPath.Clear();
            if (!TryFindPath(type, pathCandidate, ignoreCase, foundPath))
                continue;

            // match again to respect ignoreCase parameter
            if (ignoredNames.Contains(foundPath[0].Name))
                continue;

            memberPath = new NonEmptyMemberPath(type, foundPath);
            return true;
        }

        memberPath = null;
        return false;
    }

    internal bool TryFindMemberPath(ITypeSymbol type, IReadOnlyCollection<string> path, [NotNullWhen(true)] out MemberPath? memberPath)
    {
        var foundPath = new List<IMappableMember>();
        if (TryFindPath(type, path, false, foundPath))
        {
            memberPath = MemberPath.Create(type, foundPath);
            return true;
        }

        memberPath = null;
        return false;
    }

    private bool TryFindPath(ITypeSymbol type, IEnumerable<string> path, bool ignoreCase, ICollection<IMappableMember> foundPath)
    {
        foreach (var name in path)
        {
            // get T if type is Nullable<T>, prevents Value being treated as a member
            var actualType = type.NonNullableValueType() ?? type;
            if (GetMappableMember(actualType, name, ignoreCase) is not { } member)
                return false;

            type = member.Type;
            foundPath.Add(member);
        }

        return true;
    }

    public IMappableMember? GetMappableMember(ITypeSymbol symbol, string name, bool ignoreCase = false)
    {
        var membersBySymbol = ignoreCase ? _allAccessibleMembersCaseInsensitive : _allAccessibleMembersCaseSensitive;

        if (membersBySymbol.TryGetValue(symbol, out var symbolMembers))
            return symbolMembers.GetValueOrDefault(name);

        var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        membersBySymbol[symbol] = symbolMembers = GetAllAccessibleMappableMembers(symbol)
            .GroupBy(x => x.Name, comparer)
            .ToDictionary(x => x.Key, x => x.First(), comparer);
        return symbolMembers.GetValueOrDefault(name);
    }

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
        if (symbol.IsTupleType && symbol is INamedTypeSymbol namedType)
        {
            return namedType.TupleElements.Select(x => MappableMember.Create(this, x)).WhereNotNull();
        }

        // member must be property or a none backing variable field
        return GetAllMembers(symbol)
            .Where(x => x is { IsStatic: false, Kind: SymbolKind.Property } or IFieldSymbol { IsStatic: false, AssociatedSymbol: null })
            .DistinctBy(x => x.Name)
            .Select(x => MappableMember.Create(this, x))
            .WhereNotNull();
    }
}
