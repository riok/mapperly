using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static class SymbolExtensions
{
    internal static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
        => symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

    internal static bool IsImmutable(this ISymbol symbol)
        => symbol is INamedTypeSymbol namedSymbol && (namedSymbol.IsReadOnly || namedSymbol.SpecialType == SpecialType.System_String);

    internal static bool IsAccessible(this ISymbol symbol, bool allowProtected = false)
        => symbol.DeclaredAccessibility.HasFlag(Accessibility.Internal)
            || symbol.DeclaredAccessibility.HasFlag(Accessibility.Public)
            || (symbol.DeclaredAccessibility.HasFlag(Accessibility.Protected) && allowProtected);

    internal static bool HasAccessibleParameterlessConstructor(this ITypeSymbol symbol, bool allowProtected = false)
        => symbol is INamedTypeSymbol { IsAbstract: false } namedTypeSymbol
            && namedTypeSymbol.Constructors.Any(c => c.Parameters.IsDefaultOrEmpty && c.IsAccessible(allowProtected));

    internal static bool IsArrayType(this ITypeSymbol symbol)
        => symbol is IArrayTypeSymbol;

    internal static bool IsEnum(this ITypeSymbol t)
        => TryGetEnumUnderlyingType(t, out _);

    internal static bool TryGetEnumUnderlyingType(this ITypeSymbol t, [NotNullWhen(true)] out INamedTypeSymbol? enumType)
    {
        enumType = (t.NonNullable() as INamedTypeSymbol)?.EnumUnderlyingType;
        return enumType != null;
    }

    internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, string name)
        => symbol.GetAllMembers(name, StringComparer.Ordinal);

    internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, string name, IEqualityComparer<string> comparer)
    {
        var members = symbol.GetMembers().Where(x => comparer.Equals(name, x.Name));
        return symbol.BaseType == null
            ? members
            : members.Concat(symbol.BaseType.GetAllMembers(name, comparer));
    }

    internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol)
    {
        var members = symbol.GetMembers();
        return symbol.BaseType == null
            ? members
            : members.Concat(symbol.BaseType.GetAllMembers());
    }

    internal static bool ImplementsGeneric(
        this ITypeSymbol t,
        INamedTypeSymbol genericInterfaceSymbol,
        [NotNullWhen(true)] out INamedTypeSymbol? genericIntf)
    {
        if (SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, genericInterfaceSymbol))
        {
            genericIntf = (INamedTypeSymbol)t;
            return true;
        }

        genericIntf = t.AllInterfaces.FirstOrDefault(x => x.IsGenericType && SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericInterfaceSymbol));
        return genericIntf != null;
    }
}
