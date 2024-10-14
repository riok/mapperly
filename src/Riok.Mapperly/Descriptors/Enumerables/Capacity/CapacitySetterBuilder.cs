using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Enumerables.Capacity;

/// <summary>
/// Generates an <see cref="ICapacitySetter"/> of types <see cref="NonEnumeratedCapacitySetter"/> or <see cref="SimpleCapacitySetter"/> depending on type information.
/// </summary>
public static class CapacitySetterBuilder
{
    private const string CapacityMemberName = "Capacity";
    private const string TryGetNonEnumeratedCountMethodName = "TryGetNonEnumeratedCount";

    public static ICapacitySetter? TryBuildCapacitySetter(
        MappingBuilderContext ctx,
        CollectionInfos collectionInfos,
        bool includeTargetCount
    )
    {
        var source = collectionInfos.Source;
        var target = collectionInfos.Target;
        var capacitySetter = BuildCapacitySetter(ctx, target);
        if (capacitySetter == null)
            return null;

        var targetCount = includeTargetCount ? target.CountMember?.BuildGetter(ctx.UnsafeAccessorContext) : null;

        // if source count is known, create a simple EnsureCapacity statement
        if (source.CountIsKnown)
        {
            var sourceCount = source.CountMember.BuildGetter(ctx.UnsafeAccessorContext);
            return new SimpleCapacitySetter(capacitySetter, targetCount, sourceCount);
        }

        var nonEnumeratedCountMethod = ctx
            .Types.Get(typeof(Enumerable))
            .GetMembers(TryGetNonEnumeratedCountMethodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(x =>
                x.ReturnType.SpecialType == SpecialType.System_Boolean
                && x is { IsStatic: true, Parameters.Length: 2, IsGenericMethod: true }
            );

        // if non enumerated method doesnt exist then don't create EnsureCapacity
        if (nonEnumeratedCountMethod == null)
            return null;

        // if source does not have a count use GetNonEnumeratedCount, calling EnsureCapacity if count is available
        return new NonEnumeratedCapacitySetter(capacitySetter, targetCount, nonEnumeratedCountMethod);
    }

    private static IMemberSetter? BuildCapacitySetter(MappingBuilderContext ctx, CollectionInfo target)
    {
        var ensureCapacityMethod = ctx
            .SymbolAccessor.GetAllMethods(target.Type, EnsureCapacityMethodSetter.EnsureCapacityMethodName)
            .FirstOrDefault(x => x.Parameters is [{ Type.SpecialType: SpecialType.System_Int32 }] && !x.IsStatic);
        if (ensureCapacityMethod != null)
            return EnsureCapacityMethodSetter.Instance;

        var member = ctx.SymbolAccessor.GetMappableMember(target.Type, CapacityMemberName);
        if (member is { CanSetDirectly: true, IsInitOnly: false, Type.SpecialType: SpecialType.System_Int32 })
            return member.BuildSetter(ctx.UnsafeAccessorContext);

        return null;
    }
}
