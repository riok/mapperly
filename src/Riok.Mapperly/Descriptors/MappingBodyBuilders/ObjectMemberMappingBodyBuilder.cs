using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Mapping body builder for object member mappings.
/// </summary>
public static class ObjectMemberMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, IMemberAssignmentTypeMapping mapping)
    {
        var mappingCtx = new MembersContainerBuilderContext<IMemberAssignmentTypeMapping>(ctx, mapping);
        BuildMappingBody(mappingCtx);

        // init only members should not result in unmapped diagnostics for existing target mappings
        foreach (var initOnlyTargetMember in mappingCtx.EnumerateUnmappedTargetMembers().Where(x => x.IsInitOnly))
        {
            mappingCtx.SetTargetMemberMapped(initOnlyTargetMember);
        }

        // do not report "no member mapping" for existing target mappings
        mappingCtx.MappingAdded();

        mappingCtx.AddDiagnostics();
    }

    public static void BuildMappingBody(IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx)
    {
        foreach (var targetMember in ctx.EnumerateUnmappedOrConfiguredTargetMembers())
        {
            foreach (var memberMappingInfo in ctx.MatchMembers(targetMember))
            {
                BuildMemberAssignmentMapping(ctx, memberMappingInfo);
            }
        }
    }

    [SuppressMessage("Meziantou.Analyzer", "MA0051:MethodIsTooLong")]
    public static bool ValidateMappingSpecification(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberInfo,
        bool allowInitOnlyMember = false
    )
    {
        var sourceMemberPath = memberInfo.SourceMember;
        var targetMemberPath = memberInfo.TargetMember;

        // the target member path is readonly or not accessible
        if (!targetMemberPath.Member.CanSet)
        {
            // If the mapping is matched automatically without any configuration
            // mark both members as mapped,
            // as the "CannotMapToReadOnlyMember" diagnostic should be informative.
            // Also, an additional diagnostic may not even be expected here:
            // for example, if the source and target contain the same computed read-only member.
            if (memberInfo.IsAutoMatch && sourceMemberPath?.Type == SourceMemberType.Member)
            {
                ctx.SetMembersMapped(memberInfo);
                return false;
            }

            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToReadOnlyMember,
                memberInfo.DescribeSource(),
                targetMemberPath.ToDisplayString(includeMemberType: false)
            );
            return false;
        }

        // cannot access non public member in initializer
        if (allowInitOnlyMember && !targetMemberPath.Member.CanSetDirectly)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToReadOnlyMember,
                memberInfo.DescribeSource(),
                targetMemberPath.ToDisplayString(includeMemberType: false)
            );
            return false;
        }

        // a target member path part is write only or not accessible
        // an expressions target member path is only accessible with unsafe access
        if (
            targetMemberPath.ObjectPath.Any(p => !p.CanGet)
            || (ctx.BuilderContext.IsExpression && targetMemberPath.ObjectPath.Any(p => !p.CanGetDirectly))
        )
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToWriteOnlyMemberPath,
                memberInfo.DescribeSource(),
                targetMemberPath.ToDisplayString()
            );
            return false;
        }

        // cannot assign to intermediate value type, error CS1612
        // invalid mapping a value type has a property set
        if (!ValidateStructModification(ctx, memberInfo))
            return false;

        // a target member path part is init only
        var noInitOnlyPath = allowInitOnlyMember ? targetMemberPath.ObjectPath : targetMemberPath.Path;
        if (noInitOnlyPath.Any(p => p.IsInitOnly))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToInitOnlyMemberPath,
                memberInfo.DescribeSource(),
                targetMemberPath.ToDisplayString(includeMemberType: false)
            );
            return false;
        }

        // a source member path is write only or not accessible
        // an expressions source member path is only accessible with unsafe access
        if (
            sourceMemberPath != null
            && (
                sourceMemberPath.MemberPath.Path.Any(p => !p.CanGet)
                || (ctx.BuilderContext.IsExpression && sourceMemberPath.MemberPath.Path.Any(p => !p.CanGetDirectly))
            )
        )
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapFromWriteOnlyMember,
                sourceMemberPath.MemberPath.ToDisplayString(),
                targetMemberPath.ToDisplayString()
            );
            return false;
        }

        return true;
    }

    private static bool ValidateStructModification(IMembersBuilderContext<IMapping> ctx, MemberMappingInfo memberInfo)
    {
        if (memberInfo.TargetMember.Path.Count <= 1)
            return true;

        // iterate backwards, if a reference type property is found then path is valid
        // if a value type property is found then invalid, a temporary struct is being modified
        for (var i = memberInfo.TargetMember.Path.Count - 2; i >= 0; i--)
        {
            var member = memberInfo.TargetMember.Path[i];
            if (member is PropertyMember { Type: { IsValueType: true, IsRefLikeType: false } })
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.CannotMapToTemporarySourceMember,
                    memberInfo.DescribeSource(),
                    memberInfo.TargetMember.ToDisplayString(),
                    member.Type,
                    member.Name
                );
                return false;
            }

            if (member is PropertyMember { Type.IsReferenceType: true })
                break;
        }

        return true;
    }

    private static void BuildMemberAssignmentMapping(
        IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx,
        MemberMappingInfo memberMappingInfo
    )
    {
        // consume member configs
        // to ensure no further mappings are created for these configurations,
        // even if a mapping validation fails
        ctx.ConsumeMemberConfigs(memberMappingInfo);

        if (TryAddExistingTargetMapping(ctx, memberMappingInfo))
            return;

        if (!ValidateMappingSpecification(ctx, memberMappingInfo))
            return;

        if (!MemberMappingBuilder.TryBuildContainerAssignment(ctx, memberMappingInfo, out var requiresNullHandling, out var mapping))
            return;

        if (requiresNullHandling)
        {
            ctx.AddNullDelegateMemberAssignmentMapping(mapping);
        }
        else
        {
            ctx.AddMemberAssignmentMapping(mapping);
        }
    }

    private static bool TryAddExistingTargetMapping(
        IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx,
        MemberMappingInfo memberMappingInfo
    )
    {
        // can only map with an existing target from a source member
        if (memberMappingInfo.SourceMember == null)
            return false;

        var sourceMemberPath = memberMappingInfo.SourceMember;
        var targetMemberPath = memberMappingInfo.TargetMember;

        // if the member is readonly
        // and the target and source path is readable,
        // we try to create an existing target mapping
        if (
            targetMemberPath.Member is { CanSet: true, IsInitOnly: false }
            || !targetMemberPath.Path.All(op => op.CanGet)
            || !sourceMemberPath.MemberPath.Path.All(op => op.CanGet)
        )
        {
            return false;
        }

        var existingTargetMapping = ctx.BuilderContext.FindOrBuildExistingTargetMapping(memberMappingInfo.ToTypeMappingKey());
        if (existingTargetMapping == null)
            return false;

        var sourceMemberGetter = sourceMemberPath.MemberPath.BuildGetter(ctx.BuilderContext);
        var targetMemberGetter = targetMemberPath.BuildGetter(ctx.BuilderContext);
        var memberMapping = new MemberExistingTargetMapping(
            existingTargetMapping,
            sourceMemberGetter,
            targetMemberGetter,
            memberMappingInfo
        );
        ctx.AddMemberAssignmentMapping(memberMapping);
        return true;
    }
}
