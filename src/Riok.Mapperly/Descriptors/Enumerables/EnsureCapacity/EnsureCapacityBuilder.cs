using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

/// <summary>
/// Generates an <see cref="EnsureCapacity?"/> of types <see cref="EnsureCapacityNonEnumerated"/> or <see cref="EnsureCapacityMember"/> depending on type information.
/// </summary>
public static class EnsureCapacityBuilder
{
    private const string EnsureCapacityName = "EnsureCapacity";
    private const string CountPropertyName = nameof(ICollection<object>.Count);
    private const string LengthPropertyName = nameof(Array.Length);
    private const string TryGetNonEnumeratedCountMethodName = "TryGetNonEnumeratedCount";

    public static EnsureCapacity? TryBuildEnsureCapacity(ITypeSymbol sourceType, ITypeSymbol targetType, WellKnownTypes types)
    {
        var capacityMethod = targetType
            .GetAllMethods(EnsureCapacityName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(x => x.Parameters.Length == 1 && x.Parameters[0].Type.SpecialType == SpecialType.System_Int32 && !x.IsStatic);

        // if EnsureCapacity is not available then return null
        if (capacityMethod == null)
            return null;

        // if target does not have a count then return null
        if (!TryGetNonEnumeratedCount(targetType, types, out var targetSizeProperty))
            return null;

        // if target and source count are known then create a simple EnsureCapacity statement
        if (TryGetNonEnumeratedCount(sourceType, types, out var sourceSizeProperty))
            return new EnsureCapacityMember(targetSizeProperty, sourceSizeProperty);

        sourceType.ImplementsGeneric(types.IEnumerableT, out var iEnumerable);

        var nonEnumeratedCountMethod = types.Enumerable
            .GetMembers(TryGetNonEnumeratedCountMethodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(
                x => x.ReturnType.SpecialType == SpecialType.System_Boolean && x.IsStatic && x.Parameters.Length == 2 && x.IsGenericMethod
            );

        // if source does not have a count use GetNonEnumeratedCount, calling EnusureCapacity if count is available
        var typedNonEnumeratedCount = nonEnumeratedCountMethod.Construct(iEnumerable!.TypeArguments.ToArray());
        return new EnsureCapacityNonEnumerated(targetSizeProperty, typedNonEnumeratedCount);
    }

    private static bool TryGetNonEnumeratedCount(ITypeSymbol value, WellKnownTypes types, [NotNullWhen(true)] out string? expression)
    {
        if (value.IsArrayType())
        {
            expression = LengthPropertyName;
            return true;
        }

        if (value.ImplementsGeneric(types.ICollectionT, CountPropertyName, out _, out var hasCollectionCount) && !hasCollectionCount)
        {
            expression = CountPropertyName;
            return true;
        }

        if (value.ImplementsGeneric(types.IReadOnlyCollectionT, CountPropertyName, out _, out var hasReadOnlyCount) && !hasReadOnlyCount)
        {
            expression = CountPropertyName;
            return true;
        }

        expression = null;
        return false;
    }
}
