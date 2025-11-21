using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static class SymbolExtensions
{
    private static readonly SymbolDisplayFormat _fullyQualifiedNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    private static readonly ImmutableHashSet<string> _wellKnownImmutableNamespaces = ImmutableHashSet.Create(
        "System.Collections.Immutable"
    );

    private static readonly ImmutableHashSet<string> _wellKnownImmutableTypes = ImmutableHashSet.Create(
        "System.Uri",
        "System.Version",
        "System.DateTime",
        "System.DateTimeOffset",
        "System.DateOnly",
        "System.TimeOnly",
        "System.TimeSpan",
        "System.DBNull",
        "System.Void"
    );

    internal static Location? GetSyntaxLocation(this ISymbol symbol) =>
        symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation();

    internal static string FullyQualifiedIdentifierName(this ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(_fullyQualifiedNullableFormat);

    internal static bool IsImmutable(this ISymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
            return false;

        return namedSymbol.IsUnmanagedType
            || namedSymbol.SpecialType == SpecialType.System_String
            || _wellKnownImmutableTypes.Contains(namedSymbol.ToDisplayString())
            || (
                namedSymbol.ContainingNamespace != null
                && _wellKnownImmutableNamespaces.Contains(namedSymbol.ContainingNamespace.ToDisplayString())
            )
            || IsDelegate(symbol);
    }

    internal static bool IsDelegate(this ISymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
            return false;

        return namedSymbol.DelegateInvokeMethod != null;
    }

    internal static int GetInheritanceLevel(this ITypeSymbol symbol)
    {
        var level = 0;
        while (symbol.BaseType != null)
        {
            symbol = symbol.BaseType;
            level++;
        }

        return level;
    }

    internal static bool IsArrayType(this ITypeSymbol symbol) => symbol.TypeKind == TypeKind.Array;

    internal static bool IsArrayType(this ITypeSymbol symbol, [NotNullWhen(true)] out IArrayTypeSymbol? arrayTypeSymbol)
    {
        arrayTypeSymbol = symbol as IArrayTypeSymbol;
        return arrayTypeSymbol != null;
    }

    internal static bool IsEnum(this ITypeSymbol t) => TryGetEnumUnderlyingType(t, out _);

    internal static bool TryGetEnumUnderlyingType(this ITypeSymbol t, [NotNullWhen(true)] out INamedTypeSymbol? enumType)
    {
        enumType = (t.NonNullable() as INamedTypeSymbol)?.EnumUnderlyingType;
        return enumType != null;
    }

    internal static IEnumerable<IFieldSymbol> GetFields(this ITypeSymbol symbol) => symbol.GetMembers().OfType<IFieldSymbol>();

    internal static IMethodSymbol? GetStaticGenericMethod(this INamedTypeSymbol namedType, string methodName)
    {
        return namedType.GetMembers(methodName).OfType<IMethodSymbol>().FirstOrDefault(m => m.IsStatic && m.IsGenericMethod);
    }

    internal static bool ExtendsOrImplements(this ITypeSymbol t, ITypeSymbol targetSymbol) =>
        t.Extends(targetSymbol) || t.Implements((INamedTypeSymbol)targetSymbol);

    internal static bool Implements(this ITypeSymbol t, INamedTypeSymbol interfaceSymbol) =>
        SymbolEqualityComparer.Default.Equals(t, interfaceSymbol)
        || t.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, interfaceSymbol));

    internal static bool Extends(this ITypeSymbol t, ITypeSymbol targetSymbol)
    {
        for (var baseType = t; baseType != null; baseType = baseType.BaseType)
        {
            if (!SymbolEqualityComparer.Default.Equals(baseType, targetSymbol))
                continue;

            return true;
        }

        return false;
    }

    internal static bool ExtendsOrImplementsGeneric(
        this ITypeSymbol t,
        INamedTypeSymbol genericSymbol,
        [NotNullWhen(true)] out INamedTypeSymbol? typedGenericSymbol
    )
    {
        return genericSymbol.TypeKind == TypeKind.Interface
            ? t.ImplementsGeneric(genericSymbol, out typedGenericSymbol)
            : t.ExtendsGeneric(genericSymbol, out typedGenericSymbol);
    }

    internal static bool ExtendsGeneric(
        this ITypeSymbol t,
        ITypeSymbol genericSymbol,
        [NotNullWhen(true)] out INamedTypeSymbol? typedGenericSymbol
    )
    {
        if (SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, genericSymbol))
        {
            typedGenericSymbol = (INamedTypeSymbol)t;
            return true;
        }

        for (var baseType = t.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            if (!SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, genericSymbol))
                continue;

            typedGenericSymbol = baseType;
            return true;
        }

        typedGenericSymbol = null;
        return false;
    }

    internal static bool ImplementsGeneric(
        this ITypeSymbol t,
        INamedTypeSymbol genericInterfaceSymbol,
        [NotNullWhen(true)] out INamedTypeSymbol? typedInterface
    )
    {
        Debug.Assert(genericInterfaceSymbol.IsGenericType);
        Debug.Assert(genericInterfaceSymbol.TypeKind == TypeKind.Interface);

        if (SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, genericInterfaceSymbol))
        {
            typedInterface = (INamedTypeSymbol)t;
            return true;
        }

        foreach (var typeSymbol in t.AllInterfaces)
        {
            if (typeSymbol.IsGenericType && SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, genericInterfaceSymbol))
            {
                typedInterface = typeSymbol;
                return true;
            }
        }

        typedInterface = null;
        return false;
    }

    internal static bool ImplementsGeneric(
        this ITypeSymbol t,
        INamedTypeSymbol genericInterfaceSymbol,
        string symbolName,
        [NotNullWhen(true)] out INamedTypeSymbol? typedInterface,
        out bool isExplicit
    )
    {
        Debug.Assert(genericInterfaceSymbol.IsGenericType);
        Debug.Assert(genericInterfaceSymbol.TypeKind == TypeKind.Interface);

        if (SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, genericInterfaceSymbol))
        {
            typedInterface = (INamedTypeSymbol)t;
            isExplicit = false;
            return true;
        }

        typedInterface = t.AllInterfaces.FirstOrDefault(x =>
            x.IsGenericType && SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericInterfaceSymbol)
        );

        if (typedInterface == null)
        {
            isExplicit = false;
            return false;
        }

        if (t.IsAbstract)
        {
            isExplicit = false;
            return true;
        }

        var interfaceSymbol = typedInterface.GetMembers(symbolName).First();
        var symbolImplementation = t.FindImplementationForInterfaceMember(interfaceSymbol);

        // if null then the method is unimplemented
        // symbol implements genericInterface but has not implemented the corresponding methods
        // this is for example the case for arrays (arrays implement several interfaces at runtime)
        // and unit tests for which not the full interface is implemented
        if (symbolImplementation == null)
        {
            isExplicit = false;
            return false;
        }

        // check if symbol is explicit
        isExplicit = symbolImplementation switch
        {
            IMethodSymbol methodSymbol => methodSymbol.ExplicitInterfaceImplementations.Any(),
            IPropertySymbol propertySymbol => propertySymbol.ExplicitInterfaceImplementations.Any(),
            _ => throw new NotSupportedException(symbolImplementation.GetType().Name + " is not supported"),
        };

        return true;
    }

    internal static bool HasImplicitGenericImplementation(this ITypeSymbol symbol, INamedTypeSymbol inter, string methodName) =>
        symbol.ImplementsGeneric(inter, methodName, out _, out var isExplicit) && !isExplicit;

    internal static IEnumerable<ITypeSymbol> WalkTypeHierarchy(this ITypeSymbol symbol)
    {
        yield return symbol;
        while (symbol.BaseType != null)
        {
            yield return symbol.BaseType;
            symbol = symbol.BaseType;
        }
    }

    internal static bool IsInRootNamespace(this ISymbol symbol, string ns)
    {
        var namespaceSymbol = symbol.ContainingNamespace;
        while (namespaceSymbol?.ContainingNamespace is { IsGlobalNamespace: false })
        {
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        return namespaceSymbol != null && string.Equals(namespaceSymbol.Name, ns, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Returns the keyword if the type can be named by it.
    ///     For example, <see langword="uint"/> for symbol with special type <see cref="SpecialType.System_UInt32"/>
    ///     or <see langword="bool"/> for symbol with special type <see cref="SpecialType.System_Boolean"/>
    /// </summary>
    /// <param name="typeSymbol">The type</param>
    /// <param name="keywordName">The keyword</param>
    /// <returns><see langword="true"/> if the type can be named by keyword, otherwise <see langword="false"/></returns>
    internal static bool HasKeyword(this ITypeSymbol typeSymbol, [NotNullWhen(true)] out string? keywordName)
    {
        if (typeSymbol.SpecialType is >= SpecialType.System_Boolean and <= SpecialType.System_String)
        {
            keywordName = typeSymbol.ToDisplayString();
            return true;
        }

        keywordName = null;
        return false;
    }
}
