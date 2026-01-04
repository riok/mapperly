using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors;

public class SymbolAccessor(CompilationContext compilationContext, INamedTypeSymbol mapperSymbol)
{
    private const string GlobalPrefix = "global::";

    // this is a weak reference table
    // since if there is no reference to the key
    // the values should not be kept in the memory anymore / are not needed anymore.
    private readonly ConditionalWeakTable<ITypeSymbol, ITypeSymbol> _originalNullableTypes = new();
    private readonly Dictionary<ISymbol, ImmutableArray<AttributeData>> _attributes = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyCollection<ISymbol>> _allMembers = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ITypeSymbol, IReadOnlyCollection<IMappableMember>> _allAccessibleMembers = new(
        SymbolEqualityComparer.Default
    );
    private readonly Dictionary<ITypeSymbol, IReadOnlyDictionary<string, IMappableMember>> _allAccessibleMembersCaseInsensitive = new(
        SymbolEqualityComparer.Default
    );
    private readonly Dictionary<ITypeSymbol, IReadOnlyDictionary<string, IMappableMember>> _allAccessibleMembersCaseSensitive = new(
        SymbolEqualityComparer.Default
    );

    private MemberVisibility _memberVisibility = MemberVisibility.AllAccessible;
    private MemberVisibility _constructorVisibility = MemberVisibility.AllAccessible;

    private Compilation Compilation => compilationContext.Compilation;

    private readonly Lazy<INamedTypeSymbol> _lazyEnumerableType = new(() =>
        compilationContext.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T)
    );
    private INamedTypeSymbol EnumerableTypeSymbol => _lazyEnumerableType.Value;

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
            Accessibility.ProtectedAndInternal => visibility.HasFlag(MemberVisibility.Protected)
                && visibility.HasFlag(MemberVisibility.Internal),
            Accessibility.Protected => visibility.HasFlag(MemberVisibility.Protected),
            Accessibility.Internal => visibility.HasFlag(MemberVisibility.Internal),
            Accessibility.ProtectedOrInternal => visibility.HasFlag(MemberVisibility.Protected)
                || visibility.HasFlag(MemberVisibility.Internal),
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

        upgradedSymbol = UpgradeInnerSymbols(symbol).WithNullableAnnotation(NullableAnnotation.Annotated);

        _originalNullableTypes.Add(upgradedSymbol, symbol);
        return true;
    }

    private ITypeSymbol UpgradeInnerSymbols(ITypeSymbol symbol)
    {
        TryUpgradeInnerSymbols(symbol, out var upgradedSymbol);
        return upgradedSymbol ?? symbol;
    }

    /// <summary>
    /// Tries to upgrade the nullability of generic type arguments and array element type from <see cref="NullableAnnotation.None"/> to <see cref="NullableAnnotation.Annotated"/>.
    /// If generic type parameter is notnull, it will be upgraded to <see cref="NullableAnnotation.NotAnnotated"/>
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="upgradedSymbol">The symbol with upgraded inner symbols.</param>
    /// <returns>Whether an upgrade has taken place.</returns>
    private bool TryUpgradeInnerSymbols(ITypeSymbol symbol, [NotNullWhen(true)] out ITypeSymbol? upgradedSymbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol { TypeArguments.Length: > 0 } namedSymbol:
                var upgradedTypeArguments = UpgradeGenericTypeArguments(namedSymbol.TypeParameters, namedSymbol.TypeArguments);
                upgradedSymbol = namedSymbol.ConstructedFrom.Construct(
                    upgradedTypeArguments,
                    upgradedTypeArguments.Select(ta => ta.NullableAnnotation).ToImmutableArray()
                );
                return true;

            case IArrayTypeSymbol { ElementType.IsValueType: false, ElementNullableAnnotation: NullableAnnotation.None } arrayTypeSymbol:
                upgradedSymbol = Compilation.CreateArrayTypeSymbol(
                    UpgradeNullable(arrayTypeSymbol.ElementType),
                    arrayTypeSymbol.Rank,
                    NullableAnnotation.Annotated
                );
                return true;

            default:
                upgradedSymbol = default;
                return false;
        }
    }

    /// <summary>
    /// Upgrades the nullability of generic type arguments.
    /// If notnull constraint is applied to the parameter, symbol will be upgraded to <see cref="NullableAnnotation.NotAnnotated"/>,
    /// otherwise to <see cref="NullableAnnotation.Annotated"/>.
    /// </summary>
    /// <param name="typeParameters">The type parameters of the generic type.</param>
    /// <param name="typeArguments">The type arguments of the generic type.</param>
    /// <returns>Upgraded generic type arguments.</returns>
    private ImmutableArray<ITypeSymbol> UpgradeGenericTypeArguments(
        ImmutableArray<ITypeParameterSymbol> typeParameters,
        ImmutableArray<ITypeSymbol> typeArguments
    )
    {
        Debug.Assert(typeArguments.Length == typeParameters.Length);
        Debug.Assert(typeArguments.Length > 0);

        var arguments = new ITypeSymbol[typeArguments.Length];
        for (var i = 0; i < typeArguments.Length; i++)
        {
            var typeParameter = typeParameters[i];
            var typeArgument = typeArguments[i];

            var upgradedSymbol = typeParameter switch
            {
                { HasNotNullConstraint: true } => UpgradeInnerSymbols(typeArgument).WithNullableAnnotation(NullableAnnotation.NotAnnotated),
                _ => UpgradeNullable(typeArgument),
            };

            arguments[i] = upgradedSymbol;
        }

        return arguments.ToImmutableArray();
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
        where T : Attribute => GetAttributes<T>(GetAttributesCore(symbol));

    internal IEnumerable<AttributeData> TryGetAttributes<T>(IEnumerable<AttributeData> attributes)
        where T : Attribute
    {
        var attributeSymbol = compilationContext.Types.TryGet(typeof(T).FullName ?? "<unknown>");
        return attributeSymbol == null ? [] : GetAttributes(attributeSymbol, attributes);
    }

    internal IEnumerable<AttributeData> GetAttributes<T>(IEnumerable<AttributeData> attributes)
        where T : Attribute
    {
        var attributeSymbol = compilationContext.Types.Get<T>();
        return GetAttributes(attributeSymbol, attributes);
    }

    internal IEnumerable<AttributeData> GetAttributes(ITypeSymbol attributeSymbol, IEnumerable<AttributeData> attributes)
    {
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

    internal bool TryHasAttribute<T>(IEnumerable<AttributeData> symbol)
        where T : Attribute => TryGetAttributes<T>(symbol).Any();

    internal IEnumerable<IMethodSymbol> GetAllMethods(ITypeSymbol symbol) => GetAllMembers(symbol).OfType<IMethodSymbol>();

    internal IEnumerable<IMethodSymbol> GetAllMethods(ITypeSymbol symbol, string name) =>
        GetAllMembers(symbol).Where(x => string.Equals(x.Name, name, StringComparison.Ordinal)).OfType<IMethodSymbol>();

    internal IEnumerable<IFieldSymbol> GetAllFields(ITypeSymbol symbol) => GetAllMembers(symbol).OfType<IFieldSymbol>();

    internal Dictionary<object, IFieldSymbol> GetEnumFieldsByValue(ITypeSymbol symbol) =>
        GetAllFields(symbol).GroupBy(x => x.ConstantValue!).ToDictionary(f => f.Key, f => f.First());

    public IFieldSymbol? GetField(ITypeSymbol symbol, string name) =>
        GetAllFields(symbol).FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.Ordinal));

    internal HashSet<IFieldSymbol> GetFieldsExcept(ITypeSymbol symbol, ISet<IFieldSymbol> ignoredMembers) =>
        GetAllFields(symbol).Where(x => !ignoredMembers.Remove(x)).ToHashSet(SymbolTypeEqualityComparer.FieldDefault);

    internal IEnumerable<IMethodSymbol> GetAllDirectlyAccessibleMethods(ITypeSymbol symbol)
    {
        return GetAllMembers(symbol).OfType<IMethodSymbol>().Where(IsDirectlyAccessible);
    }

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
        IEnumerable<StringMemberPath> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? memberPath
    )
    {
        var foundPath = new List<IMappableMember>();
        foreach (var pathCandidate in pathCandidates)
        {
            if (!members.TryGetValue(pathCandidate.Path[0], out var member))
                continue;

            foundPath.Clear();
            foundPath.Add(member);
            if (pathCandidate.Path.Count != 1 && !TryFindPath(member.Type, pathCandidate.SkipRoot(), ignoreCase, foundPath))
                continue;

            memberPath = new NonEmptyMemberPath(member.Type, foundPath);
            return true;
        }

        memberPath = null;
        return false;
    }

    internal bool TryFindMemberPath(
        IEnumerable<ITypeSymbol> types,
        IEnumerable<StringMemberPath> pathCandidates,
        IReadOnlyCollection<string> ignoredNames,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? memberPath
    )
    {
        var paths = pathCandidates.ToList();
        foreach (var type in types)
        {
            if (TryFindMemberPath(type, paths, ignoredNames, ignoreCase, out memberPath))
            {
                return true;
            }
        }

        memberPath = null;
        return false;
    }

    internal bool TryFindMemberPath(
        ITypeSymbol type,
        IEnumerable<StringMemberPath> pathCandidates,
        IReadOnlyCollection<string> ignoredNames,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? memberPath
    )
    {
        var foundPath = new List<IMappableMember>();
        foreach (var pathCandidate in pathCandidates)
        {
            // fast path for exact case matches
            if (ignoredNames.Contains(pathCandidate.Path[0]))
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

    internal bool TryFindMemberPath(ITypeSymbol type, IMemberPathConfiguration path, [NotNullWhen(true)] out MemberPath? memberPath)
    {
        if (path is StringMemberPath stringMemberPath)
            return TryFindMemberPath(type, stringMemberPath, out memberPath);

        // resolve from symbol member path
        // if it is not possible to resolve by direct symbols
        // the string path is tried to ensure backwards compatibility
        // (e.g. when the A.MyValue is referenced,
        // but instead B.MyValue is the correct one,
        // with the string representation it doesn't matter, it is just MyValue).
        var symbolMemberPath = (SymbolMemberPath)path;
        var memberPathSegments = new List<IMappableMember>(symbolMemberPath.PathCount);
        foreach (var pathSegment in symbolMemberPath.Path)
        {
            if (MappableMember.Create(this, pathSegment) is { } mappableMember)
            {
                memberPathSegments.Add(mappableMember);
                continue;
            }

            return TryFindMemberPath(type, symbolMemberPath.ToStringMemberPath(), out memberPath);
        }

        var nameOfRefType = memberPathSegments[0].ContainingType;
        if (nameOfRefType == null || !CanAssign(type, nameOfRefType))
        {
            return TryFindMemberPath(type, symbolMemberPath.ToStringMemberPath(), out memberPath);
        }

        memberPath = new NonEmptyMemberPath(type, memberPathSegments);
        return true;
    }

    internal bool TryFindMemberPath(ITypeSymbol type, StringMemberPath path, [NotNullWhen(true)] out MemberPath? memberPath)
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

    /// <summary>
    ///     Checks that the specified method returns a type that can be assigned to the specified result type,
    ///     and that the specified arguments can be passed to the method.
    ///     The check takes into account the possibility of assignment to a method with an argument marked with the <see langword="params"/> keyword
    /// </summary>
    /// <param name="method">Method for validate</param>
    /// <param name="returnType">Target return type</param>
    /// <param name="argTypes">Target method arguments</param>
    /// <returns></returns>
    internal bool ValidateSignature(IMethodSymbol method, ITypeSymbol returnType, params ITypeSymbol[] argTypes)
    {
        return CanAssign(method.ReturnType, returnType) && Enumerable.Range(0, method.Parameters.Length).All(IsValidParameter);

        bool IsValidParameter(int i)
        {
            if (method.Parameters[i] is not { IsParams: true } isParamsParameter)
            {
                return CanAssign(argTypes[i], method.Parameters[i].Type);
            }

            // see https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters#params-modifier

            var argsToEnd = argTypes.AsSpan(i);

            // for empty args aka Call(params X[]) as Call()
            if (argsToEnd.IsEmpty)
            {
                return true;
            }

            var elementType = isParamsParameter.Type.ImplementsGeneric(EnumerableTypeSymbol, out var impl)
                // for assignable to IEnumerable<T>
                ? impl.TypeArguments.First()
                // for Span<T> and ReadOnlySpan<T>
                : ((INamedTypeSymbol)method.Parameters[i].Type).TypeArguments.First();

            //for single arg aka Call(X[]) or Call(X)
            if (argsToEnd.Length == 1)
            {
                return CanAssign(argsToEnd[0], method.Parameters[i].Type) || CanAssign(argsToEnd[0], elementType);
            }

            // for multiple args
            foreach (var typeSymbol in argsToEnd)
            {
                if (!CanAssign(typeSymbol, elementType))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private bool TryFindPath(ITypeSymbol type, StringMemberPath path, bool ignoreCase, ICollection<IMappableMember> foundPath)
    {
        foreach (var name in path.Path)
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

    public TOperation? GetOperation<TOperation>(SyntaxNode node)
        where TOperation : class, IOperation => compilationContext.GetSemanticModel(node.SyntaxTree)?.GetOperation(node) as TOperation;

    public INamedTypeSymbol? GetTypeByMetadataName(string targetTypeName)
    {
        var startsWithGlobal = targetTypeName.StartsWith(GlobalPrefix, StringComparison.Ordinal);
        if (startsWithGlobal)
        {
            targetTypeName = targetTypeName[GlobalPrefix.Length..];
        }

        return Compilation.GetBestTypeByMetadataName(targetTypeName);
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
