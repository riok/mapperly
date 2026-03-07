using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

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
        mappingCtx.AddDiagnostics(true);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, NewInstanceObjectMemberMethodMapping mapping)
    {
        var mappingCtx = new NewInstanceContainerBuilderContext<NewInstanceObjectMemberMethodMapping>(ctx, mapping);
        BuildConstructorMapping(mappingCtx);
        BuildInitMemberMappings(mappingCtx);
        ObjectMemberMappingBodyBuilder.BuildMappingBody(mappingCtx);
        mappingCtx.AddDiagnostics(true);
    }

    public static IReadOnlyList<ConstructorParameterMapping> BuildConstructorMapping(
        INewInstanceBuilderContext<INewInstanceObjectMemberMapping> ctx,
        bool? preferParameterlessConstructor = null
    )
    {
        if (ctx.Mapping.HasConstructor)
            return [];

        if (ctx.Mapping.TargetType is not INamedTypeSymbol namedTargetType)
        {
            ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
            return [];
        }

        // attributed ctor is prio 1
        // if preferParameterlessConstructors is true (default) :parameterless ctor is prio 2 then by descending parameter count
        // the reverse if preferParameterlessConstructors is false , descending parameter count is prio2 then parameterless ctor
        // ctors annotated with [Obsolete] are considered last unless they have a MapperConstructor attribute set
        var ctorCandidates = namedTargetType
            .InstanceConstructors.Where(ctor => ctx.BuilderContext.SymbolAccessor.IsConstructorAccessible(ctor))
            .OrderByDescending(x => ctx.BuilderContext.SymbolAccessor.HasAttribute<MapperConstructorAttribute>(x))
            .ThenBy(x => ctx.BuilderContext.SymbolAccessor.HasAttribute<ObsoleteAttribute>(x));

        if (preferParameterlessConstructor ?? ctx.BuilderContext.Configuration.Mapper.PreferParameterlessConstructors)
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

            ctx.Mapping.Constructor = ctx.BuilderContext.InstanceConstructors.BuildForConstructor(ctorCandidate);

            foreach (var mapping in constructorParameterMappings)
            {
                ctx.AddConstructorParameterMapping(mapping);
            }

            return constructorParameterMappings;
        }

        ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
        ctx.Mapping.Constructor = new InstanceConstructor(namedTargetType);
        return [];
    }

    public static void BuildInitMemberMappings(
        INewInstanceBuilderContext<INewInstanceObjectMemberMapping> ctx,
        bool includeAllMembers = false
    )
    {
        if (!ctx.Mapping.Constructor.SupportsObjectInitializer)
            return;

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
            if (
                targetMember.IsRequired
                || ctx.BuilderContext.Configuration.HasRequiredMappingStrategyForMembers(RequiredMappingStrategy.Target)
            )
            {
                ctx.BuilderContext.ReportDiagnostic(
                    targetMember.IsRequired ? DiagnosticDescriptors.RequiredMemberNotMapped : DiagnosticDescriptors.SourceMemberNotFound,
                    targetMember.Name,
                    ctx.Mapping.TargetType,
                    ctx.Mapping.SourceType
                );
            }
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
        ctx.ConsumeMemberConfigs(memberInfo);

        if (!ObjectMemberMappingBodyBuilder.ValidateMappingSpecification(ctx, memberInfo, true))
            return;

        // For non-required init members with ThrowOnPropertyMappingNullMismatch disabled
        // and a nullable source mapped to a non-nullable target:
        // skip the init mapping entirely to preserve the target's own default initializer.
        // This is consistent with set properties, which use an if-guard
        // and skip the assignment when the source is null.
        if (ShouldSkipNullableInitMember(ctx, memberInfo))
        {
            ReportNullMismatchDiagnostic(ctx, memberInfo);
            ctx.SetMembersMapped(memberInfo);
            return;
        }

        if (!MemberMappingBuilder.TryBuildAssignment(ctx, memberInfo, out var memberAssignmentMapping))
            return;

        ctx.AddInitMemberMapping(memberAssignmentMapping);
    }

    /// <summary>
    /// Determines whether a non-required init member should be skipped when the source is nullable
    /// and the target is non-nullable, and ThrowOnPropertyMappingNullMismatch is disabled.
    /// Skipping omits the property from the object initializer, preserving the target's own default.
    /// </summary>
    private static bool ShouldSkipNullableInitMember(
        INewInstanceBuilderContext<INewInstanceObjectMemberMapping> ctx,
        MemberMappingInfo memberInfo
    )
    {
        // Required init members must always be in the object initializer.
        if (memberInfo.TargetMember.Member.IsRequired)
            return false;

        // IQueryable<T> projections ignore ThrowOnPropertyMappingNullMismatch.
        if (ctx.BuilderContext.IsExpression)
            return false;

        // When ThrowOnPropertyMappingNullMismatch is enabled,
        // the user expects a throw on null; keep the init mapping.
        if (ctx.BuilderContext.Configuration.Mapper.ThrowOnPropertyMappingNullMismatch)
            return false;

        // Only relevant when source is nullable and target is non-nullable.
        if (!memberInfo.IsSourceNullable || memberInfo.TargetMember.MemberType.IsNullable())
            return false;

        // If a delegate mapping already handles the null-to-non-null conversion,
        // no inline null handling would be generated and the init member should be kept.
        if (memberInfo.SourceMember != null)
        {
            var mappingKey = memberInfo.ToTypeMappingKey();
            var delegateMapping = ctx.BuilderContext.FindOrBuildLooseNullableMapping(
                mappingKey,
                diagnosticLocation: memberInfo.Configuration?.Location
            );
            if (delegateMapping != null && delegateMapping.SourceType.IsNullable() && !delegateMapping.TargetType.IsNullable())
            {
                return false;
            }
        }

        return true;
    }

    private static void ReportNullMismatchDiagnostic(
        INewInstanceBuilderContext<INewInstanceObjectMemberMapping> ctx,
        MemberMappingInfo memberInfo
    )
    {
        if (memberInfo.Configuration?.SuppressNullMismatchDiagnostic == true)
            return;

        if (memberInfo.SourceMember == null)
            return;

        ctx.BuilderContext.ReportDiagnostic(
            DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
            memberInfo.SourceMember.MemberPath.FullName,
            memberInfo.SourceMember.MemberPath.RootType.ToDisplayString(),
            memberInfo.TargetMember.FullName,
            memberInfo.TargetMember.RootType.ToDisplayString()
        );
    }

    private static bool TryBuildConstructorMapping(
        INewInstanceBuilderContext<IMapping> ctx,
        IMethodSymbol ctor,
        [NotNullWhen(true)] out List<ConstructorParameterMapping>? constructorParameterMappings
    )
    {
        constructorParameterMappings = [];

        var skippedOptionalParam = false;
        foreach (var parameter in ctor.Parameters)
        {
            if (
                !ctx.TryMatchParameter(parameter, out var memberMappingInfo)
                || !SourceValueBuilder.TryBuildMappedSourceValue(ctx, memberMappingInfo, out var sourceValue)
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
