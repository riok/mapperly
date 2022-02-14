using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static class SymbolExtensions
{
    internal static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
        => symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

    internal static bool IsAccessible(this ISymbol symbol)
        => symbol.DeclaredAccessibility.HasFlag(Accessibility.Protected)
            || symbol.DeclaredAccessibility.HasFlag(Accessibility.Internal)
            || symbol.DeclaredAccessibility.HasFlag(Accessibility.Public);

    internal static bool HasAccessibleParameterlessConstructor(this ITypeSymbol symbol)
        => symbol is INamedTypeSymbol { IsAbstract: false } namedTypeSymbol
            && namedTypeSymbol.Constructors.Any(c => c.Parameters.IsDefaultOrEmpty && c.IsAccessible());

    internal static bool IsArrayType(this ITypeSymbol symbol)
        => symbol is IArrayTypeSymbol;

    internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, string name)
    {
        var members = symbol.GetMembers(name);
        return symbol.BaseType == null
            ? members
            : members.Concat(symbol.BaseType.GetAllMembers(name));
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
