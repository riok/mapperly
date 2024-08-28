using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

public static class NullableSymbolExtensions
{
    internal static bool HasSameOrStricterNullability(this ITypeSymbol symbol, ITypeSymbol other)
    {
        return symbol.NullableAnnotation == NullableAnnotation.NotAnnotated || symbol.NullableAnnotation == other.NullableAnnotation;
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

    internal static bool IsNullable(this ITypeSymbol symbol) => symbol.IsNullableReferenceType() || symbol.IsNullableValueType();

    internal static bool IsNullableReferenceType(this ITypeSymbol symbol) => symbol.NullableAnnotation.IsNullable();

    internal static bool IsNullableValueType(this ITypeSymbol symbol) => symbol.NonNullableValueType() != null;

    internal static ITypeSymbol? NonNullableValueType(this ITypeSymbol symbol)
    {
        if (symbol.IsValueType && symbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
            return namedType.TypeArguments[0];
        return null;
    }

    /// <summary>
    /// Whether or not the <see cref="ITypeParameterSymbol"/> is nullable.
    /// </summary>
    /// <param name="typeParameter">The type parameter.</param>
    /// <returns>A boolean indicating whether <c>null</c> can be used to satisfy the type parameter constraints. Null means unspecified.</returns>
    internal static bool? IsNullable(this ITypeParameterSymbol typeParameter)
    {
        if (typeParameter.NullableAnnotation != NullableAnnotation.NotAnnotated)
            return true;

        if (typeParameter.HasNotNullConstraint || typeParameter.HasValueTypeConstraint || typeParameter.HasUnmanagedTypeConstraint)
            return false;

        bool? fallback = null;

        if (typeParameter.HasReferenceTypeConstraint)
        {
            if (!typeParameter.ReferenceTypeConstraintNullableAnnotation.IsNullable())
                return false;

            fallback = true;
        }

        if (typeParameter.ConstraintTypes.Length > 0)
        {
            foreach (var constraint in typeParameter.ConstraintTypes)
            {
                if (!constraint.IsNullable())
                    return false;

                fallback = true;
            }
        }

        return fallback;
    }

    internal static bool IsNullableUpgraded(this ITypeSymbol symbol)
    {
        if (symbol.NullableAnnotation == NullableAnnotation.None)
            return false;

        return symbol switch
        {
            INamedTypeSymbol namedTypeSymbol when namedTypeSymbol.TypeArguments.Any(x => !x.IsNullableUpgraded()) => false,
            IArrayTypeSymbol arrayTypeSymbol when !arrayTypeSymbol.ElementType.IsNullableUpgraded() => false,
            _ => true,
        };
    }

    internal static bool IsNullable(this NullableAnnotation nullable) =>
        nullable is NullableAnnotation.Annotated or NullableAnnotation.None;

    internal static NullableAnnotation Upgrade(this NullableAnnotation nullable) =>
        nullable.IsNullable() ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated;
}
