using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
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
        var ignoreCase = ctx.BuilderContext.MapperConfiguration.PropertyNameMappingStrategy == PropertyNameMappingStrategy.CaseInsensitive;

        foreach (var targetMember in ctx.TargetMembers.Values)
        {
            if (ctx.MemberConfigsByRootTargetName.Remove(targetMember.Name, out var memberConfigs))
            {
                // add all configured mappings
                // order by target path count to map less nested items first (otherwise they would overwrite all others)
                // eg. target.A = source.B should be mapped before target.A.Id = source.B.Id
                foreach (var config in memberConfigs.OrderBy(x => x.Target.Path.Count))
                {
                    BuildMemberAssignmentMapping(ctx, config);
                }

                continue;
            }

            if (
                ctx.BuilderContext.SymbolAccessor.TryFindMemberPath(
                    ctx.Mapping.SourceType,
                    MemberPathCandidateBuilder.BuildMemberPathCandidates(targetMember.Name),
                    ctx.IgnoredSourceMemberNames,
                    ignoreCase,
                    out var sourceMemberPath
                )
            )
            {
                BuildMemberAssignmentMapping(ctx, sourceMemberPath, new MemberPath(new[] { targetMember }));
                continue;
            }

            if (
                targetMember.CanSet
                && ctx.BuilderContext.Configuration.Properties.RequiredMappingStrategy.HasFlag(RequiredMappingStrategy.Target)
            )
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
        PropertyMappingConfiguration config
    )
    {
        if (!ctx.BuilderContext.SymbolAccessor.TryFindMemberPath(ctx.Mapping.TargetType, config.Target.Path, out var targetMemberPath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound,
                config.Target.FullName,
                ctx.Mapping.TargetType
            );
            return;
        }

        if (!ctx.BuilderContext.SymbolAccessor.TryFindMemberPath(ctx.Mapping.SourceType, config.Source.Path, out var sourceMemberPath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingSourceMemberNotFound,
                config.Source.FullName,
                ctx.Mapping.SourceType
            );
            return;
        }

        BuildMemberAssignmentMapping(ctx, sourceMemberPath, targetMemberPath);
    }

    [SuppressMessage(" Meziantou.Analyzer", "MA0051:MethodIsTooLong")]
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

        // cannot access non public member in initializer
        if (
            allowInitOnlyMember
            && (
                !ctx.BuilderContext.SymbolAccessor.IsDirectlyAccessible(targetMemberPath.Member.MemberSymbol)
                || !targetMemberPath.Member.CanSetDirectly
            )
        )
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
        // an expressions target member path is only accessible with unsafe access
        if (
            targetMemberPath.ObjectPath.Any(p => !p.CanGet)
            || (
                ctx.BuilderContext.IsExpression
                && targetMemberPath.ObjectPath.Any(p => !ctx.BuilderContext.SymbolAccessor.IsDirectlyAccessible(p.MemberSymbol))
            )
        )
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

        // cannot assign to intermediate value type, error CS1612
        // invalid mapping a value type has a property set
        if (!ValidateStructModification(ctx, sourceMemberPath, targetMemberPath))
            return false;

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
        // an expressions source member path is only accessible with unsafe access
        if (
            sourceMemberPath.Path.Any(p => !p.CanGet)
            || (
                ctx.BuilderContext.IsExpression
                && sourceMemberPath.Path.Any(p => !ctx.BuilderContext.SymbolAccessor.IsDirectlyAccessible(p.MemberSymbol))
            )
        )
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

    private static bool ValidateStructModification(
        IMembersBuilderContext<IMapping> ctx,
        MemberPath sourceMemberPath,
        MemberPath targetMemberPath
    )
    {
        if (targetMemberPath.Path.Count <= 1)
            return true;

        // iterate backwards, if a reference type property is found then path is valid
        // if a value type property is found then invalid, a temporary struct is being modified
        for (var i = targetMemberPath.Path.Count - 2; i >= 0; i--)
        {
            var member = targetMemberPath.Path[i];
            if (member is PropertyMember { Type: { IsValueType: true, IsRefLikeType: false } })
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.CannotMapToTemporarySourceMember,
                    ctx.Mapping.SourceType,
                    sourceMemberPath.FullName,
                    sourceMemberPath.Member.Type,
                    ctx.Mapping.TargetType,
                    targetMemberPath.FullName,
                    targetMemberPath.Member.Type,
                    member.Name,
                    member.Type
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

        var getterSourcePath = GetterMemberPath.Build(ctx.BuilderContext, sourceMemberPath);
        var setterTargetPath = SetterMemberPath.Build(ctx.BuilderContext, targetMemberPath);

        // no member of the source path is nullable, no null handling needed
        if (!sourceMemberPath.IsAnyNullable())
        {
            var memberMapping = new MemberMapping(delegateMapping, getterSourcePath, false, true);
            ctx.AddMemberAssignmentMapping(new MemberAssignmentMapping(setterTargetPath, memberMapping));
            return;
        }

        // If null property assignments are allowed,
        // and the delegate mapping accepts nullable types (and converts it to a non-nullable type),
        // or the mapping is synthetic and the target accepts nulls
        // access the source in a null save matter (via ?.) but no other special handling required.
        if (
            ctx.BuilderContext.MapperConfiguration.AllowNullPropertyAssignment
            && (delegateMapping.SourceType.IsNullable() || delegateMapping.IsSynthetic && targetMemberPath.Member.IsNullable)
        )
        {
            var memberMapping = new MemberMapping(delegateMapping, getterSourcePath, true, false);
            ctx.AddMemberAssignmentMapping(new MemberAssignmentMapping(setterTargetPath, memberMapping));
            return;
        }

        // additional null condition check
        // (only map if source is not null, else may throw depending on settings)
        ctx.AddNullDelegateMemberAssignmentMapping(
            new MemberAssignmentMapping(setterTargetPath, new MemberMapping(delegateMapping, getterSourcePath, false, true))
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

        var getterSourcePath = GetterMemberPath.Build(ctx.BuilderContext, sourceMemberPath);
        var setterTargetPath = GetterMemberPath.Build(ctx.BuilderContext, targetMemberPath);

        var memberMapping = new MemberExistingTargetMapping(existingTargetMapping, getterSourcePath, setterTargetPath);
        ctx.AddMemberAssignmentMapping(memberMapping);
        return true;
    }
}
