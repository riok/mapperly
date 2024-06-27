using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

internal static class EnumerableMappingBodyBuilder
{
    private const string SystemNamespaceName = "System";

    public static void BuildMappingBody(MappingBuilderContext ctx, INewInstanceEnumerableMapping mapping)
    {
        var mappingCtx = new NewInstanceContainerBuilderContext<INewInstanceEnumerableMapping>(ctx, mapping);
        InitContext(mappingCtx);
        BuildConstructorMapping(mappingCtx);
        NewInstanceObjectMemberMappingBodyBuilder.BuildInitMemberMappings(mappingCtx);
        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics();
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, IEnumerableMapping mapping)
    {
        var mappingCtx = new MembersContainerBuilderContext<IEnumerableMapping>(ctx, mapping);
        InitContext(mappingCtx);

        if (EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx, mapping.CollectionInfos) is { } ensureCapacity)
        {
            mapping.AddEnsureCapacity(ensureCapacity);
        }

        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics();
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
        // ignore all members of collection classes of the System.Private.CoreLib assembly or of arrays
        // as these are considered mapped by the enumerable mapping itself
        // these members can still be mapped with an explicit configuration.
        var systemType = type.WalkTypeHierarchy().FirstOrDefault(x => x.IsArrayType() || x.IsInRootNamespace(SystemNamespaceName));
        if (systemType == null)
            return;

        foreach (var member in ctx.BuilderContext.SymbolAccessor.GetAllAccessibleMappableMembers(systemType))
        {
            ctx.IgnoreMembers(member.Name);
        }
    }

    private static void BuildConstructorMapping(INewInstanceBuilderContext<INewInstanceEnumerableMapping> ctx)
    {
        // allow source count being mapped to a target constructor parameter
        // named with a well known "count" name
        var additionalCtorParameterMappings = new Dictionary<string, MemberPath>(3, StringComparer.OrdinalIgnoreCase);
        if (
            ctx.Mapping.CollectionInfos.Source.CountIsKnown
            && ctx.BuilderContext.SymbolAccessor.TryFindMemberPath(
                ctx.Mapping.SourceType,
                [ctx.Mapping.CollectionInfos.Source.CountPropertyName],
                out var sourceCountMemberPath
            )
        )
        {
            additionalCtorParameterMappings[nameof(List<object>.Capacity)] = sourceCountMemberPath;
            additionalCtorParameterMappings[nameof(List<object>.Count)] = sourceCountMemberPath;
            additionalCtorParameterMappings[nameof(Array.Length)] = sourceCountMemberPath;
        }

        // always prefer parameterized constructor for system collections (to map capacity correctly)
        var targetIsSystemType = ctx.Mapping.TargetType.IsArrayType() || ctx.Mapping.TargetType.IsInRootNamespace(SystemNamespaceName);

        var options = new NewInstanceObjectMemberMappingBodyBuilder.ConstructorMappingBuilderOptions(
            additionalCtorParameterMappings,
            PreferParameterlessConstructor: targetIsSystemType ? false : null
        );
        var usedParameterValues = NewInstanceObjectMemberMappingBodyBuilder.BuildConstructorMapping(ctx, options);

        // if no additional parameter was used,
        // the count/capacity is not mapped,
        // try to build an EnsureCapacity statement.
        if (
            usedParameterValues.Count == 0
            && EnsureCapacityBuilder.TryBuildEnsureCapacity(ctx.BuilderContext, ctx.Mapping.CollectionInfos) is { } ensureCapacity
        )
        {
            if (ctx.BuilderContext.CollectionInfos!.Source.CountIsKnown)
            {
                ctx.SetMembersMapped(ctx.BuilderContext.CollectionInfos.Source.CountPropertyName);
            }

            ctx.Mapping.AddEnsureCapacity(ensureCapacity);
        }
    }
}
