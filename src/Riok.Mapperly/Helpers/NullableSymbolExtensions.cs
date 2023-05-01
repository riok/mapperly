using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

public static class NullableSymbolExtensions
{
    private const string NullableGenericTypeName = "System.Nullable<T>";

    internal static bool HasSameOrStricterNullability(this ITypeSymbol symbol, ITypeSymbol other)
    {
        return symbol.NullableAnnotation == NullableAnnotation.NotAnnotated
            || symbol.UpgradeNullable().NullableAnnotation == other.UpgradeNullable().NullableAnnotation;
    }

    /// <summary>
    /// Upgrade the nullability of a symbol from <see cref="NullableAnnotation.None"/> to <see cref="NullableAnnotation.Annotated"/>.
    /// Does not upgrade the nullability of type parameters or array element types.
    /// </summary>
    /// <param name="symbol">The symbol to upgrade.</param>
    /// <returns>The upgraded symbol</returns>
    internal static ITypeSymbol UpgradeNullable(this ITypeSymbol symbol)
    {
        TryUpgradeNullable(symbol, out var upgradedSymbol);
        return upgradedSymbol ?? symbol;
    }

    /// <summary>
    /// Tries to upgrade the nullability of a symbol from <see cref="NullableAnnotation.None"/> to <see cref="NullableAnnotation.Annotated"/>.
    /// Does not upgrade the nullability of type parameters or array element types.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="upgradedSymbol">The upgraded symbol, if an upgrade has taken place, <c>null</c> otherwise.</param>
    /// <returns>Whether an upgrade has taken place.</returns>
    internal static bool TryUpgradeNullable(this ITypeSymbol symbol, [NotNullWhen(true)] out ITypeSymbol? upgradedSymbol)
    {
        if (symbol.NullableAnnotation != NullableAnnotation.None)
        {
            upgradedSymbol = default;
            return false;
        }

        upgradedSymbol = symbol.WithNullableAnnotation(NullableAnnotation.Annotated);
        return true;
    }

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

    /// <summary>
    /// If the <see cref="symbol"/> is a value type,
    /// this is a no-op. For any other value,
    /// the non-nullable variant is returned.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    internal static ITypeSymbol NonNullableReferenceType(this ITypeSymbol symbol)
    {
        return symbol.IsValueType ? symbol : symbol.NonNullable();
    }

    internal static bool IsNullable(this ITypeSymbol symbol) =>
        symbol.NullableAnnotation.IsNullable() || symbol.NonNullableValueType() is not null;

    internal static bool IsNullableValueType(this ITypeSymbol symbol) => symbol.NonNullableValueType() is not null;

    internal static bool IsNullable(this NullableAnnotation nullable) =>
        nullable is NullableAnnotation.Annotated or NullableAnnotation.None;

    private static ITypeSymbol? NonNullableValueType(this ITypeSymbol symbol)
    {
        if (
            symbol is INamedTypeSymbol { IsValueType: true, IsGenericType: true } namedType
            && namedType.ConstructedFrom.ToDisplayString() == NullableGenericTypeName
        )
            return namedType.TypeArguments[0];
        return null;
    }
}
