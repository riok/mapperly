using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Helpers;

internal static class SymbolExtensions
{
    internal static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol) =>
        symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

    internal static bool IsImmutable(this ISymbol symbol) =>
        symbol is INamedTypeSymbol namedSymbol && (namedSymbol.IsUnmanagedType || namedSymbol.SpecialType == SpecialType.System_String);

    internal static bool IsAccessible(this ISymbol symbol, bool allowProtected = false) =>
        symbol.DeclaredAccessibility.HasFlag(Accessibility.Internal)
        || symbol.DeclaredAccessibility.HasFlag(Accessibility.Public)
        || (symbol.DeclaredAccessibility.HasFlag(Accessibility.Protected) && allowProtected);

    internal static bool HasAccessibleParameterlessConstructor(this ITypeSymbol symbol, bool allowProtected = false) =>
        symbol is INamedTypeSymbol { IsAbstract: false } namedTypeSymbol
        && namedTypeSymbol.Constructors.Any(c => c.Parameters.IsDefaultOrEmpty && c.IsAccessible(allowProtected));

    internal static bool IsArrayType(this ITypeSymbol symbol) => symbol is IArrayTypeSymbol;

    internal static bool IsEnum(this ITypeSymbol t) => TryGetEnumUnderlyingType(t, out _);

    internal static bool TryGetEnumUnderlyingType(this ITypeSymbol t, [NotNullWhen(true)] out INamedTypeSymbol? enumType)
    {
        enumType = (t.NonNullable() as INamedTypeSymbol)?.EnumUnderlyingType;
        return enumType != null;
    }

    internal static IEnumerable<IMethodSymbol> GetAllMethods(this ITypeSymbol symbol) => symbol.GetAllMembers().OfType<IMethodSymbol>();

    internal static IEnumerable<IMethodSymbol> GetAllMethods(this ITypeSymbol symbol, string name) =>
        symbol.GetAllMembers(name).OfType<IMethodSymbol>();

    internal static IEnumerable<IPropertySymbol> GetAllProperties(this ITypeSymbol symbol, string name) =>
        symbol.GetAllMembers(name).OfType<IPropertySymbol>();

    internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol)
    {
        var members = symbol.GetMembers();

        if (symbol.TypeKind == TypeKind.Interface)
        {
            var interfaceProperties = symbol.AllInterfaces.SelectMany(i => i.GetAllMembers());
            return members.Concat(interfaceProperties);
        }

        return symbol.BaseType == null ? members : members.Concat(symbol.BaseType.GetAllMembers());
    }

    internal static IEnumerable<IMappableMember> GetMappableMembers(
        this ITypeSymbol symbol,
        string name,
        IEqualityComparer<string> comparer
    )
    {
        return symbol.GetAllMembers().Where(x => !x.IsStatic && comparer.Equals(name, x.Name)).Select(MappableMember.Create).WhereNotNull();
    }

    internal static IEnumerable<IMappableMember> GetAccessibleMappableMembers(this ITypeSymbol symbol)
    {
        return symbol
            .GetAllMembers()
            .Where(x => !x.IsStatic && x.IsAccessible())
            .DistinctBy(x => x.Name)
            .Select(MappableMember.Create)
            .WhereNotNull();
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

        var symbolImplementaton = t.FindImplementationForInterfaceMember(interfaceSymbol);

        // if null then the method is unimplemented
        // symbol implements genericInterface but has not implemented the corresponding methods
        // this can only occur in unit tests
        if (symbolImplementaton == null)
            throw new NotSupportedException("Symbol implementation cannot be null for objects implementing interface.");

        // check if symbol is explicit
        isExplicit = symbolImplementaton switch
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
        compilation.ClassifyConversion(symbol, type).IsImplicit;

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

    private static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol, string name) =>
        symbol.GetAllMembers().Where(x => name.Equals(x.Name));
}
