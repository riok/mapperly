using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Helpers;

internal static class SymbolExtensions
{
    private static readonly SymbolDisplayFormat _fullyQualifiedNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );

    private static readonly ImmutableHashSet<string> _wellKnownImmutableTypes = ImmutableHashSet.Create(
        typeof(Uri).FullName!,
        typeof(Version).FullName!
    );

    internal static string FullyQualifiedIdentifierName(this ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(_fullyQualifiedNullableFormat);

    internal static bool IsImmutable(this ISymbol symbol) =>
        symbol is INamedTypeSymbol namedSymbol
        && (
            namedSymbol.IsUnmanagedType
            || namedSymbol.SpecialType == SpecialType.System_String
            || _wellKnownImmutableTypes.Contains(namedSymbol.ToDisplayString())
        );

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

    internal static IEnumerable<IFieldSymbol> GetFields(this ITypeSymbol symbol) => symbol.GetMembers().OfType<IFieldSymbol>();

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
            _ => throw new NotSupportedException(symbolImplementation.GetType().Name + " is not supported"),
        };

        return true;
    }

    internal static bool HasImplicitGenericImplementation(this ITypeSymbol symbol, INamedTypeSymbol inter, string methodName) =>
        symbol.ImplementsGeneric(inter, methodName, out _, out var isExplicit) && !isExplicit;

    // Method to check if a given ITypeSymbol is a primitive type or an enumerable of primitives
    internal static bool IsPrimitiveOrEnumerableOfPrimitives(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
        {
            return false;
        }

        if (IsPrimitiveType(typeSymbol))
        {
            return true;
        }

        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol && IsPrimitiveType(arrayTypeSymbol.ElementType))
        {
            return true;
        }

        if (
            typeSymbol is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && typeSymbol.AllInterfaces.Any(s => s.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            && namedTypeSymbol.TypeArguments.All(s => IsPrimitiveType(s))
        )
        {
            return true;
        }

        return false;
    }

    // Method to check if a given ITypeSymbol is a primitive type
    private static bool IsPrimitiveType(ITypeSymbol? typeSymbol)
    {
        return typeSymbol != null
            && (
                typeSymbol.TypeKind == TypeKind.Enum
                || typeSymbol.GetTypedConstantKind()
                || string.Equals(typeSymbol.ContainingNamespace?.Name, "System", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static bool GetTypedConstantKind(this ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Byte:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Char:
            case SpecialType.System_String:
            case SpecialType.System_DateTime:
            case SpecialType.System_ValueType:
                return true;
            default:
                return false;
        }
    }
}
