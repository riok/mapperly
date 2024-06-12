using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Body builder for new instance object member mappings (mappings for which the target object gets created via <code>new()</code>).
/// </summary>
public static class NewInstanceObjectMemberMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, NewInstanceObjectMemberMapping mapping)
    {
        var mappingCtx = new NewInstanceBuilderContext<NewInstanceObjectMemberMapping>(ctx, mapping);
        BuildConstructorMapping(mappingCtx);
        BuildInitMemberMappings(mappingCtx, true);
        mappingCtx.AddDiagnostics();
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, NewInstanceObjectMemberMethodMapping mapping)
    {
        var mappingCtx = new NewInstanceContainerBuilderContext<NewInstanceObjectMemberMethodMapping>(ctx, mapping);
        BuildConstructorMapping(mappingCtx);
        BuildInitMemberMappings(mappingCtx);
        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics();
    }

    private static void BuildInitMemberMappings(
        INewInstanceBuilderContext<INewInstanceObjectMemberMapping> ctx,
        bool includeAllMembers = false
    )
    {
        var initOnlyTargetMembers = includeAllMembers
            ? ctx.EnumerateUnmappedTargetMembers().ToArray()
            : ctx.EnumerateUnmappedTargetMembers().Where(x => x.CanOnlySetViaInitializer()).ToArray();
        foreach (var targetMember in initOnlyTargetMembers)
        {
            if (ctx.TryMatchInitOnlyMember(targetMember, out var memberInfo))
            {
                BuildInitMemberMapping(ctx, memberInfo);
                continue;
            }

            // set the member mapped as it is an init only member
            // diagnostics are already reported
            // and no further mapping attempts should be undertaken
            ctx.BuilderContext.ReportDiagnostic(
                targetMember.IsRequired ? DiagnosticDescriptors.RequiredMemberNotMapped : DiagnosticDescriptors.SourceMemberNotFound,
                targetMember.Name,
                ctx.Mapping.TargetType,
                ctx.Mapping.SourceType
            );
            ctx.SetTargetMemberMapped(targetMember);
        }
    }

    private static void BuildInitMemberMapping(
        INewInstanceBuilderContext<INewInstanceObjectMemberMapping> ctx,
        MemberMappingInfo memberInfo
    )
    {
        // consume member configs
        // to ensure no further mappings are created for these configurations,
        // even if a mapping validation fails
        ctx.ConsumeMemberConfig(memberInfo);

        if (!ObjectMemberMappingBodyBuilder.ValidateMappingSpecification(ctx, memberInfo, true))
            return;

        if (!MemberMappingBuilder.TryBuildAssignment(ctx, memberInfo, out var memberAssignmentMapping))
            return;

        ctx.AddInitMemberMapping(memberAssignmentMapping);
    }

    private static void BuildConstructorMapping(INewInstanceBuilderContext<INewInstanceObjectMemberMapping> ctx)
    {
        if (ctx.Mapping.TargetType is not INamedTypeSymbol namedTargetType)
        {
            ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
            return;
        }

        // attributed ctor is prio 1
        // if preferParameterlessConstructors is true (default) :parameterless ctor is prio 2 then by descending parameter count
        // the reverse if preferParameterlessConstructors is false , descending parameter count is prio2 then parameterless ctor
        // ctors annotated with [Obsolete] are considered last unless they have a MapperConstructor attribute set
        var ctorCandidates = namedTargetType
            .InstanceConstructors.Where(ctor => ctx.BuilderContext.SymbolAccessor.IsDirectlyAccessible(ctor))
            .OrderByDescending(x => ctx.BuilderContext.SymbolAccessor.HasAttribute<MapperConstructorAttribute>(x))
            .ThenBy(x => ctx.BuilderContext.SymbolAccessor.HasAttribute<ObsoleteAttribute>(x));

        if (ctx.BuilderContext.Configuration.Mapper.PreferParameterlessConstructors)
        {
            ctorCandidates = ctorCandidates.ThenByDescending(x => x.Parameters.Length == 0).ThenByDescending(x => x.Parameters.Length);
        }
        else
        {
            ctorCandidates = ctorCandidates.ThenByDescending(x => x.Parameters.Length).ThenByDescending(x => x.Parameters.Length == 0);
        }

        foreach (var ctorCandidate in ctorCandidates)
        {
            if (!TryBuildConstructorMapping(ctx, ctorCandidate, out var constructorParameterMappings))
            {
                if (ctx.BuilderContext.SymbolAccessor.HasAttribute<MapperConstructorAttribute>(ctorCandidate))
                {
                    ctx.BuilderContext.ReportDiagnostic(
                        DiagnosticDescriptors.CannotMapToConfiguredConstructor,
                        ctx.Mapping.SourceType,
                        ctorCandidate
                    );
                }

                continue;
            }

            foreach (var mapping in constructorParameterMappings)
            {
                ctx.AddConstructorParameterMapping(mapping);
            }

            return;
        }

        ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
    }

    private static bool TryBuildConstructorMapping(
        INewInstanceBuilderContext<IMapping> ctx,
        IMethodSymbol ctor,
        [NotNullWhen(true)] out List<ConstructorParameterMapping>? constructorParameterMappings
    )
    {
        constructorParameterMappings = new List<ConstructorParameterMapping>();
        var skippedOptionalParam = false;
        foreach (var parameter in ctor.Parameters)
        {
            if (
                !ctx.TryMatchParameter(parameter, out var memberMappingInfo)
                || !MemberMappingBuilder.TryBuild(ctx, memberMappingInfo, MemberMappingBuilder.CodeStyle.Expression, out var sourceValue)
            )
            {
                // expressions do not allow skipping of optional parameters
                if (!parameter.IsOptional || ctx.BuilderContext.IsExpression)
                    return false;

                skippedOptionalParam = true;
                continue;
            }

            var ctorMapping = new ConstructorParameterMapping(parameter, sourceValue, skippedOptionalParam, memberMappingInfo);
            constructorParameterMappings.Add(ctorMapping);
        }

        return true;
    }
}
