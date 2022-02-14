using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

public static class NullableSymbolExtensions
{
    private const string NullableGenericTypeName = "System.Nullable<T>";

    internal static bool TryGetNonNullable(this ITypeSymbol symbol, [NotNullWhen(true)] out ITypeSymbol? nonNullable)
    {
        if (symbol.NonNullableValueType() is { } t)
        {
            nonNullable = t;
            return true;
        }

        if (symbol.NullableAnnotation.IsNullable())
        {
            nonNullable = symbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            return true;
        }

        nonNullable = default;
        return false;
    }

    internal static bool NonNullable(this ITypeSymbol symbol, out ITypeSymbol nonNullable)
    {
        if (TryGetNonNullable(symbol, out var nonNullableType))
        {
            nonNullable = nonNullableType;
            return true;
        }

        nonNullable = symbol;
        return false;
    }

    internal static ITypeSymbol NonNullable(this ITypeSymbol symbol)
    {
        NonNullable(symbol, out var nonNullable);
        return nonNullable;
    }

    internal static bool IsNullable(this ITypeSymbol symbol)
        => symbol.NullableAnnotation.IsNullable()
            || symbol.NonNullableValueType() is not null;

    internal static bool IsNullableValueType(this ITypeSymbol symbol)
        => symbol.NonNullableValueType() is not null;

    internal static bool IsNullable(this IPropertySymbol symbol)
        => symbol.NullableAnnotation.IsNullable()
            || symbol.Type.IsNullable();

    internal static bool IsNullable(this NullableAnnotation nullable)
        => nullable is NullableAnnotation.Annotated or NullableAnnotation.None;

    private static ITypeSymbol? NonNullableValueType(this ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol { IsValueType: true, IsGenericType: true } namedType &&
            namedType.ConstructedFrom.ToDisplayString() == NullableGenericTypeName)
            return namedType.TypeArguments[0];
        return null;
    }
}
