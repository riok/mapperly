using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Helpers;

internal static class SymbolExtensions
{
    private static readonly ImmutableHashSet<string> _wellKnownImmutableTypes = ImmutableHashSet.Create(
        typeof(Uri).FullName,
        typeof(Version).FullName
    );

    internal static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol, WellKnownTypes knownTypes) =>
        knownTypes.GetAttributes(symbol).Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

    internal static bool IsImmutable(this ISymbol symbol) =>
        symbol is INamedTypeSymbol namedSymbol
        && (
            namedSymbol.IsUnmanagedType
            || namedSymbol.SpecialType == SpecialType.System_String
            || _wellKnownImmutableTypes.Contains(namedSymbol.ToDisplayString())
        );

    internal static bool IsAccessible(this ISymbol symbol, bool allowProtected = false) =>
        symbol.DeclaredAccessibility.HasFlag(Accessibility.Internal)
        || symbol.DeclaredAccessibility.HasFlag(Accessibility.Public)
        || (symbol.DeclaredAccessibility.HasFlag(Accessibility.Protected) && allowProtected);

    internal static bool HasAccessibleParameterlessConstructor(this ITypeSymbol symbol, bool allowProtected = false) =>
        symbol is INamedTypeSymbol { IsAbstract: false } namedTypeSymbol
        && namedTypeSymbol.InstanceConstructors.Any(c => c.Parameters.IsDefaultOrEmpty && c.IsAccessible(allowProtected));

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

    internal static bool IsArrayType(this ITypeSymbol symbol) => symbol is IArrayTypeSymbol;

    internal static bool IsEnum(this ITypeSymbol t) => TryGetEnumUnderlyingType(t, out _);

    internal static bool TryGetEnumUnderlyingType(this ITypeSymbol t, [NotNullWhen(true)] out INamedTypeSymbol? enumType)
    {
        enumType = (t.NonNullable() as INamedTypeSymbol)?.EnumUnderlyingType;
        return enumType != null;
    }

    internal static IEnumerable<IMethodSymbol> GetAllMethods(this ITypeSymbol symbol, WellKnownTypes types) =>
        symbol.GetAllMembers(types).OfType<IMethodSymbol>();

    internal static IEnumerable<IMethodSymbol> GetAllMethods(this ITypeSymbol symbol, string name, WellKnownTypes types) =>
        symbol.GetAllMembers(name, types).OfType<IMethodSymbol>();

    internal static IEnumerable<IPropertySymbol> GetAllProperties(this ITypeSymbol symbol, string name, WellKnownTypes types) =>
        symbol.GetAllMembers(name, types).OfType<IPropertySymbol>();

    internal static IEnumerable<IFieldSymbol> GetFields(this ITypeSymbol symbol) => symbol.GetMembers().OfType<IFieldSymbol>();

    internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, WellKnownTypes types)
    {
        return types.GetAllMembers(symbol);
    }

    internal static IEnumerable<IMappableMember> GetMappableMembers(
        this ITypeSymbol symbol,
        string name,
        IEqualityComparer<string> comparer,
        WellKnownTypes types
    )
    {
        return symbol.GetAccessibleMappableMembers(types).Where(x => comparer.Equals(name, x.Name));
    }

    internal static IEnumerable<IMappableMember> GetAccessibleMappableMembers(this ITypeSymbol symbol, WellKnownTypes types)
    {
        return types.GetAllAccessibleMappableMembers(symbol);
    }

    internal static IMethodSymbol? GetStaticGenericMethod(this INamedTypeSymbol namedType, string methodName)
    {
        return namedType.GetMembers(methodName).OfType<IMethodSymbol>().FirstOrDefault(m => m.IsStatic && m.IsGenericMethod);
    }

    internal static bool ImplementsGeneric(
        this ITypeSymbol t,
        INamedTypeSymbol genericInterfaceSymbol,
        [NotNullWhen(true)] out INamedTypeSymbol? typedInterface
    )
    {
        if (SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, genericInterfaceSymbol))
        {
            typedInterface = (INamedTypeSymbol)t;
            return true;
        }

        typedInterface = t.AllInterfaces.FirstOrDefault(
            x => x.IsGenericType && SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericInterfaceSymbol)
        );
        return typedInterface != null;
    }

    internal static bool ImplementsGeneric(
        this ITypeSymbol t,
        INamedTypeSymbol genericInterfaceSymbol,
        string symbolName,
        [NotNullWhen(true)] out INamedTypeSymbol? typedInterface,
        out bool isExplicit
    )
    {
        if (SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, genericInterfaceSymbol))
        {
            typedInterface = (INamedTypeSymbol)t;
            isExplicit = false;
            return true;
        }

        typedInterface = t.AllInterfaces.FirstOrDefault(
            x => x.IsGenericType && SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, genericInterfaceSymbol)
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
            _ => throw new NotImplementedException(),
        };

        return true;
    }

    internal static bool HasImplicitGenericImplementation(this ITypeSymbol symbol, INamedTypeSymbol inter, string methodName) =>
        symbol.ImplementsGeneric(inter, methodName, out _, out var isExplicit) && !isExplicit;

    internal static bool IsAssignableTo(this ITypeSymbol symbol, Compilation compilation, ITypeSymbol type) =>
        compilation.ClassifyConversion(symbol, type).IsImplicit && (type.IsNullable() || !symbol.IsNullable());

    internal static bool CanConsumeType(
        this ITypeParameterSymbol typeParameter,
        Compilation compilation,
        NullableAnnotation typeParameterUsageNullableAnnotation,
        ITypeSymbol type
    )
    {
        if (typeParameter.HasConstructorConstraint && !type.HasAccessibleParameterlessConstructor())
            return false;

        if (!typeParameter.IsNullable(typeParameterUsageNullableAnnotation) && type.IsNullable())
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

    private static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, string name, WellKnownTypes types) =>
        symbol.GetAllMembers(types).Where(x => name.Equals(x.Name));
}
