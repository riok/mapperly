using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
        => symbol.GetAllMembers().Where(x => comparer.Equals(name, x.Name));

    internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol)
    {
        var members = symbol.GetMembers();

        if (symbol.TypeKind == TypeKind.Interface)
        {
            var interfaceProperties = symbol.AllInterfaces.SelectMany(i => i.GetAllMembers());
            return members.Concat(interfaceProperties);
        }

        return symbol.BaseType == null
            ? members
            : members.Concat(symbol.BaseType.GetAllMembers());
    }

    internal static IEnumerable<IPropertySymbol> GetAllAccessibleProperties(this ITypeSymbol symbol)
    {
        return symbol
            .GetAllMembers()
            .OfType<IPropertySymbol>()
            .Where(x => x.IsAccessible())
            .DistinctBy(x => x.Name);
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

    internal static bool Implements(this ITypeSymbol t, INamedTypeSymbol interfaceSymbol)
        => t.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(interfaceSymbol, x));

    internal static bool CanConsumeType(this ITypeParameterSymbol typeParameter, Compilation compilation, ITypeSymbol type)
    {
        if (typeParameter.HasConstructorConstraint && !type.HasAccessibleParameterlessConstructor())
            return false;

        if (typeParameter.HasNotNullConstraint && type.IsNullable())
            return false;

        if (typeParameter.HasValueTypeConstraint && !type.IsValueType)
            return false;

        if (typeParameter.HasReferenceTypeConstraint && !type.IsReferenceType)
            return false;

        foreach (var constraintType in typeParameter.ConstraintTypes)
        {
            if (!compilation.ClassifyConversion(type, constraintType.UpgradeNullable()).IsImplicit)
                return false;
        }

        return true;
    }
}
