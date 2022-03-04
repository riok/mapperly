using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using NewInstanceMappingBuilderContext =
    Riok.Mapperly.Descriptors.MappingBuilder.ObjectPropertyMappingBuilderContext<
        Riok.Mapperly.Descriptors.Mappings.NewInstanceObjectPropertyMapping>;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class NewInstanceObjectPropertyMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (ctx.Target.SpecialType != SpecialType.None || ctx.Source.SpecialType != SpecialType.None)
            return null;

        if (ctx.Target is not INamedTypeSymbol namedTarget || namedTarget.Constructors.All(x => !x.IsAccessible()))
            return null;

        return new NewInstanceObjectPropertyMapping(ctx.Source, ctx.Target.NonNullable());
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, NewInstanceObjectPropertyMapping mapping)
    {
        var mappingCtx = new NewInstanceMappingBuilderContext(ctx, mapping);

        // map constructor
        if (TryBuildConstructorMapping(mappingCtx, out var mappedTargetPropertyNames))
        {
            mappingCtx.TargetProperties.RemoveRange(mappedTargetPropertyNames);
        }
        else
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.Target);
        }

        ObjectPropertyMappingBuilder.BuildMappingBody(mappingCtx);
    }

    private static bool TryBuildConstructorMapping(
        NewInstanceMappingBuilderContext ctx,
        [NotNullWhen(true)] out ISet<string>? mappedTargetPropertyNames)
    {
        if (ctx.Mapping.TargetType is not INamedTypeSymbol namedTargetType)
        {
            mappedTargetPropertyNames = null;
            return false;
        }

        var mapperConstructorAttribute = ctx.BuilderContext.GetTypeSymbol(typeof(MapperConstructorAttribute));

        // attributed ctor is prio 1
        // parameterless ctor is prio 2
        // then by descending parameter count
        var ctorCandidates = namedTargetType.Constructors
            .Where(ctor => ctor.IsAccessible())
            .OrderByDescending(x => x.HasAttribute(mapperConstructorAttribute))
            .ThenByDescending(x => x.Parameters.Length == 0)
            .ThenByDescending(x => x.Parameters.Length);
        foreach (var ctorCandidate in ctorCandidates)
        {
            if (!TryBuildConstructorMapping(
                ctx,
                ctorCandidate,
                out mappedTargetPropertyNames,
                out var constructorParameterMappings))
            {
                if (ctorCandidate.HasAttribute(mapperConstructorAttribute))
                {
                    ctx.BuilderContext.ReportDiagnostic(
                        DiagnosticDescriptors.CannotMapToConfiguredConstructor,
                        ctx.Mapping.SourceType,
                        ctorCandidate);
                }

                continue;
            }

            foreach (var constructorParameterMapping in constructorParameterMappings)
            {
                ctx.Mapping.AddConstructorParameterMapping(constructorParameterMapping);
            }

            return true;
        }

        mappedTargetPropertyNames = null;
        return false;
    }

    private static bool TryBuildConstructorMapping(
        NewInstanceMappingBuilderContext ctx,
        IMethodSymbol ctor,
        [NotNullWhen(true)] out ISet<string>? mappedTargetPropertyNames,
        [NotNullWhen(true)] out ISet<ConstructorParameterMapping>? constructorParameterMappings)
    {
        constructorParameterMappings = new HashSet<ConstructorParameterMapping>();
        mappedTargetPropertyNames = new HashSet<string>();
        var skippedOptionalParam = false;
        foreach (var parameter in ctor.Parameters)
        {
            if (!PropertyPath.TryFind(
                ctx.Mapping.SourceType,
                MemberPathCandidateBuilder.BuildMemberPathCandidates(parameter.Name),
                StringComparer.OrdinalIgnoreCase,
                out var sourcePath))
            {
                if (!parameter.IsOptional)
                    return false;

                skippedOptionalParam = true;
                continue;
            }

            // nullability is handled inside the property mapping
            var paramType = parameter.Type.WithNullableAnnotation(parameter.NullableAnnotation);
            var delegateMapping = ctx.BuilderContext.FindMapping(sourcePath.MemberType, paramType)
                ?? ctx.BuilderContext.FindOrBuildMapping(sourcePath.Member.Type.NonNullable(), paramType);
            if (delegateMapping == null)
            {
                if (!parameter.IsOptional)
                    return false;

                skippedOptionalParam = true;
                continue;
            }

            var propertyMapping = new NullPropertyMapping(delegateMapping, sourcePath, ctx.BuilderContext.GetNullFallbackValue(paramType));
            var ctorMapping = new ConstructorParameterMapping(parameter, propertyMapping, skippedOptionalParam);
            constructorParameterMappings.Add(ctorMapping);
            mappedTargetPropertyNames.Add(parameter.Name);
        }

        return true;
    }
}
