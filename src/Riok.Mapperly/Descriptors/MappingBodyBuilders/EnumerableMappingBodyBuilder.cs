using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Enumerables.Capacity;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

internal static class EnumerableMappingBodyBuilder
{
    private const string SystemNamespaceName = "System";
    private const string CapacityMemberName = "Capacity";

    private static readonly IReadOnlyCollection<string> _sourceCountAlias =
    [
        nameof(Array.Length),
        nameof(List<object>.Count),
        nameof(List<object>.Capacity),
    ];

    public static void BuildMappingBody(MappingBuilderContext ctx, INewInstanceEnumerableMapping mapping)
    {
        var mappingCtx = new NewInstanceContainerBuilderContext<INewInstanceEnumerableMapping>(ctx, mapping);
        InitContext(mappingCtx);
        BuildConstructorMapping(mappingCtx);
        NewInstanceObjectMemberMappingBodyBuilder.BuildInitMemberMappings(mappingCtx);
        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics(true);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, IEnumerableMapping mapping)
    {
        var mappingCtx = new MembersContainerBuilderContext<IEnumerableMapping>(ctx, mapping);
        InitContext(mappingCtx);

        // include the target count as the target could already include elements
        if (CapacitySetterBuilder.TryBuildCapacitySetter(ctx, mapping.CollectionInfos, true) is { } capacitySetter)
        {
            mappingCtx.IgnoreMembers(CapacityMemberName);
            mapping.AddCapacitySetter(capacitySetter);
        }
        else
        {
            IgnoreCapacityIfSystemType(mappingCtx);
        }

        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics(false);
    }

    private static void InitContext<T>(MembersMappingBuilderContext<T> ctx)
        where T : IEnumerableMapping
    {
        // the enumerable mapping itself is considered a mapping
        ctx.MappingAdded();
        IgnoreSystemMembers(ctx, ctx.Mapping.SourceType);
        IgnoreSystemMembers(ctx, ctx.Mapping.TargetType);
    }

    private static void IgnoreSystemMembers<T>(IMembersBuilderContext<T> ctx, ITypeSymbol type)
        where T : IMapping
    {
        // ignore all members of collection classes of the System.Private.CoreLib assembly
        // as these are considered mapped by the enumerable mapping itself
        // these members can still be mapped with an explicit configuration.
        var systemType = type.WalkTypeHierarchy().FirstOrDefault(x => x.IsArrayType() || x.IsInRootNamespace(SystemNamespaceName));
        if (systemType == null)
            return;

        foreach (var member in ctx.BuilderContext.SymbolAccessor.GetAllAccessibleMappableMembers(systemType))
        {
            ctx.IgnoreMembers(member);
        }
    }

    private static void IgnoreCapacityIfSystemType<T>(IMembersBuilderContext<T> ctx)
        where T : IEnumerableMapping
    {
        if (ctx.Mapping.SourceType.IsInRootNamespace(SystemNamespaceName) || ctx.Mapping.TargetType.IsInRootNamespace(SystemNamespaceName))
        {
            ctx.IgnoreMembers(CapacityMemberName);
        }
    }

    private static void BuildConstructorMapping(INewInstanceBuilderContext<INewInstanceEnumerableMapping> ctx)
    {
        // allow source count being mapped to a target constructor parameter
        // named with a well known "count" name
        if (ctx.Mapping.CollectionInfos.Source.CountIsKnown)
        {
            foreach (var countAlias in _sourceCountAlias)
            {
                ctx.TryAddSourceMemberAlias(countAlias, ctx.Mapping.CollectionInfos.Source.CountMember);
            }
        }

        // always prefer parameterized constructor for system collections (to map capacity correctly)
        var targetIsSystemType = ctx.Mapping.TargetType.IsArrayType() || ctx.Mapping.TargetType.IsInRootNamespace(SystemNamespaceName);
        var ctorParamMappings = NewInstanceObjectMemberMappingBodyBuilder.BuildConstructorMapping(ctx, targetIsSystemType ? false : null);

        var countIsMapped =
            ctx.BuilderContext.CollectionInfos!.Source.CountIsKnown
            && ctorParamMappings.Any(m =>
                Equals(m.MemberInfo.SourceMember?.MemberPath.Member, ctx.Mapping.CollectionInfos.Source.CountMember)
            );

        // if no additional parameter was used,
        // the count/capacity is not mapped,
        // try to build an EnsureCapacity statement.
        // do not include the target count as the instance is just created by the ctor
        if (
            !countIsMapped
            && CapacitySetterBuilder.TryBuildCapacitySetter(ctx.BuilderContext, ctx.Mapping.CollectionInfos, false) is { } capacitySetter
        )
        {
            if (ctx.Mapping.CollectionInfos.Source.CountIsKnown)
            {
                ctx.IgnoreMembers(ctx.Mapping.CollectionInfos.Source.CountMember);
            }

            ctx.IgnoreMembers(CapacityMemberName);
            ctx.Mapping.AddCapacitySetter(capacitySetter);
        }
        else
        {
            IgnoreCapacityIfSystemType(ctx);
        }
    }
}
