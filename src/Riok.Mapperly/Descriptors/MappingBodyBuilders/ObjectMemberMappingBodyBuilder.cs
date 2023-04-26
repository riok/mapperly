using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

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
    }

    public static void BuildMappingBody(IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx)
    {
        var memberNameComparer =
            ctx.BuilderContext.MapperConfiguration.PropertyNameMappingStrategy == PropertyNameMappingStrategy.CaseSensitive
                ? StringComparer.Ordinal
                : StringComparer.OrdinalIgnoreCase;

        foreach (var targetMember in ctx.TargetMembers.Values)
        {
            if (ctx.MemberConfigsByRootTargetName.Remove(targetMember.Name, out var memberConfigs))
            {
                // add all configured mappings
                // order by target path count to map less nested items first (otherwise they would overwrite all others)
                // eg. target.A = source.B should be mapped before target.A.Id = source.B.Id
                foreach (var config in memberConfigs.OrderBy(x => x.Target.Count))
                {
                    BuildMemberAssignmentMapping(ctx, config);
                }

                continue;
            }

            if (
                MemberPath.TryFind(
                    ctx.Mapping.SourceType,
                    MemberPathCandidateBuilder.BuildMemberPathCandidates(targetMember.Name),
                    ctx.IgnoredSourceMemberNames,
                    memberNameComparer,
                    out var sourceMemberPath
                )
            )
            {
                BuildMemberAssignmentMapping(ctx, sourceMemberPath, new MemberPath(new[] { targetMember }));
                continue;
            }

            if (targetMember.CanSet)
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.SourceMemberNotFound,
                    targetMember.Name,
                    ctx.Mapping.TargetType,
                    ctx.Mapping.SourceType
                );
            }
        }

        ctx.AddDiagnostics();
    }

    private static void BuildMemberAssignmentMapping(
        IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx,
        MapPropertyAttribute config
    )
    {
        if (!MemberPath.TryFind(ctx.Mapping.TargetType, config.Target, out var targetMemberPath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound,
                string.Join(MemberPath.MemberAccessSeparator, config.Target),
                ctx.Mapping.TargetType
            );
            return;
        }

        if (!MemberPath.TryFind(ctx.Mapping.SourceType, config.Source, out var sourceMemberPath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingSourceMemberNotFound,
                string.Join(MemberPath.MemberAccessSeparator, config.Source),
                ctx.Mapping.SourceType
            );
            return;
        }

        BuildMemberAssignmentMapping(ctx, sourceMemberPath, targetMemberPath);
    }

    public static bool ValidateMappingSpecification(
        IMembersBuilderContext<IMapping> ctx,
        MemberPath sourceMemberPath,
        MemberPath targetMemberPath,
        bool allowInitOnlyMember = false
    )
    {
        // the target member path is readonly or not accessible
        if (!targetMemberPath.Member.CanSet)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToReadOnlyMember,
                ctx.Mapping.SourceType,
                sourceMemberPath.FullName,
                sourceMemberPath.Member.Type,
                ctx.Mapping.TargetType,
                targetMemberPath.FullName,
                targetMemberPath.Member.Type
            );
            return false;
        }

        // a target member path part is write only or not accessible
        if (targetMemberPath.ObjectPath.Any(p => !p.CanGet))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToWriteOnlyMemberPath,
                ctx.Mapping.SourceType,
                sourceMemberPath.FullName,
                sourceMemberPath.Member.Type,
                ctx.Mapping.TargetType,
                targetMemberPath.FullName,
                targetMemberPath.Member.Type
            );
            return false;
        }

        // a target member path part is init only
        var noInitOnlyPath = allowInitOnlyMember ? targetMemberPath.ObjectPath : targetMemberPath.Path;
        if (noInitOnlyPath.Any(p => p.IsInitOnly))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToInitOnlyMemberPath,
                ctx.Mapping.SourceType,
                sourceMemberPath.FullName,
                sourceMemberPath.Member.Type,
                ctx.Mapping.TargetType,
                targetMemberPath.FullName,
                targetMemberPath.Member.Type
            );
            return false;
        }

        // a source member path is write only or not accessible
        if (sourceMemberPath.Path.Any(p => !p.CanGet))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapFromWriteOnlyMember,
                ctx.Mapping.SourceType,
                sourceMemberPath.FullName,
                sourceMemberPath.Member.Type,
                ctx.Mapping.TargetType,
                targetMemberPath.FullName,
                targetMemberPath.Member.Type
            );
            return false;
        }

        // cannot map from an indexed member
        if (sourceMemberPath.Member.IsIndexer)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapFromIndexedMember,
                ctx.Mapping.SourceType,
                sourceMemberPath.FullName,
                ctx.Mapping.TargetType,
                targetMemberPath.FullName
            );
            return false;
        }

        return true;
    }

    private static void BuildMemberAssignmentMapping(
        IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx,
        MemberPath sourceMemberPath,
        MemberPath targetMemberPath
    )
    {
        if (TryAddExistingTargetMapping(ctx, sourceMemberPath, targetMemberPath))
            return;

        if (!ValidateMappingSpecification(ctx, sourceMemberPath, targetMemberPath))
            return;

        // nullability is handled inside the member mapping
        var delegateMapping =
            ctx.BuilderContext.FindMapping(sourceMemberPath.Member.Type, targetMemberPath.Member.Type)
            ?? ctx.BuilderContext.FindOrBuildMapping(
                sourceMemberPath.Member.Type.NonNullable(),
                targetMemberPath.Member.Type.NonNullable()
            );

        // couldn't build the mapping
        if (delegateMapping == null)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotMapMember,
                ctx.Mapping.SourceType,
                sourceMemberPath.FullName,
                sourceMemberPath.Member.Type,
                ctx.Mapping.TargetType,
                targetMemberPath.FullName,
                targetMemberPath.Member.Type
            );
            return;
        }

        // no member of the source path is nullable, no null handling needed
        if (!sourceMemberPath.IsAnyNullable())
        {
            var memberMapping = new MemberMapping(delegateMapping, sourceMemberPath, false, true);
            ctx.AddMemberAssignmentMapping(new MemberAssignmentMapping(targetMemberPath, memberMapping));
            return;
        }

        // the source is nullable, or the mapping is a direct assignment and the target allows nulls
        // access the source in a null save matter (via ?.) but no other special handling required.
        if (delegateMapping.SourceType.IsNullable() || delegateMapping.IsSynthetic && targetMemberPath.Member.IsNullable)
        {
            var memberMapping = new MemberMapping(delegateMapping, sourceMemberPath, true, false);
            ctx.AddMemberAssignmentMapping(new MemberAssignmentMapping(targetMemberPath, memberMapping));
            return;
        }

        // additional null condition check
        // (only map if source is not null, else may throw depending on settings)
        ctx.AddNullDelegateMemberAssignmentMapping(
            new MemberAssignmentMapping(targetMemberPath, new MemberMapping(delegateMapping, sourceMemberPath, false, true))
        );
    }

    private static bool TryAddExistingTargetMapping(
        IMembersContainerBuilderContext<IMemberAssignmentTypeMapping> ctx,
        MemberPath sourceMemberPath,
        MemberPath targetMemberPath
    )
    {
        // if the member is readonly
        // and the target and source path is readable,
        // we try to create an existing target mapping
        if (targetMemberPath.Member.CanSet || !targetMemberPath.Path.All(op => op.CanGet) || !sourceMemberPath.Path.All(op => op.CanGet))
        {
            return false;
        }

        var existingTargetMapping = ctx.BuilderContext.FindOrBuildExistingTargetMapping(
            sourceMemberPath.Member.Type,
            targetMemberPath.Member.Type
        );
        if (existingTargetMapping == null)
            return false;

        var memberMapping = new MemberExistingTargetMapping(existingTargetMapping, sourceMemberPath, targetMemberPath);
        ctx.AddMemberAssignmentMapping(memberMapping);
        return true;
    }
}
