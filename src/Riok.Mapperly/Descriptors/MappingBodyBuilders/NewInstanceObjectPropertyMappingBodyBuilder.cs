using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Body builder for new instance object property mappings (mappings for which the target object gets created via <code>new()</code>).
/// </summary>
public static class NewInstanceObjectPropertyMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, NewInstanceObjectPropertyMapping mapping)
    {
        var mappingCtx = new NewInstanceBuilderContext<NewInstanceObjectPropertyMapping>(ctx, mapping);
        BuildConstructorMapping(mappingCtx);
        BuildInitOnlyPropertyMappings(mappingCtx, true);
        mappingCtx.AddDiagnostics();
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, NewInstanceObjectPropertyMethodMapping mapping)
    {
        var mappingCtx = new NewInstanceContainerBuilderContext<NewInstanceObjectPropertyMethodMapping>(ctx, mapping);
        BuildConstructorMapping(mappingCtx);
        BuildInitOnlyPropertyMappings(mappingCtx);
        ObjectPropertyMappingBodyBuilder.BuildMappingBody(mappingCtx);
    }

    private static void BuildInitOnlyPropertyMappings(INewInstanceBuilderContext<IMapping> ctx, bool includeAllProperties = false)
    {
        var initOnlyTargetProperties = includeAllProperties
            ? ctx.TargetProperties.Values.ToArray()
            : ctx.TargetProperties.Values.Where(x => x.CanOnlySetViaInitializer()).ToArray();
        foreach (var targetProperty in initOnlyTargetProperties)
        {
            ctx.TargetProperties.Remove(targetProperty.Name);

            if (ctx.PropertyConfigsByRootTargetName.Remove(targetProperty.Name, out var propertyConfigs))
            {
                BuildInitPropertyMapping(ctx, targetProperty, propertyConfigs);
                continue;
            }

            if (!PropertyPath.TryFind(
                ctx.Mapping.SourceType,
                MemberPathCandidateBuilder.BuildMemberPathCandidates(targetProperty.Name),
                ctx.IgnoredSourcePropertyNames,
                out var sourcePropertyPath))
            {
                ctx.BuilderContext.ReportDiagnostic(
                    targetProperty.IsRequired()
                        ? DiagnosticDescriptors.RequiredPropertyNotMapped
                        : DiagnosticDescriptors.SourcePropertyNotFound,
                    targetProperty.Name,
                    ctx.Mapping.TargetType,
                    ctx.Mapping.SourceType);
                continue;
            }

            BuildInitPropertyMapping(ctx, targetProperty, sourcePropertyPath);
        }
    }

    private static void BuildInitPropertyMapping(
        INewInstanceBuilderContext<IMapping> ctx,
        IPropertySymbol targetProperty,
        IReadOnlyCollection<MapPropertyAttribute> propertyConfigs)
    {
        // add configured mapping
        // target paths are not supported (yet), only target properties
        if (propertyConfigs.Count > 1)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForInitOnlyProperty,
                targetProperty.Type,
                targetProperty.Name);
        }

        var propertyConfig = propertyConfigs.First();
        if (propertyConfig.Target.Count > 1)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.InitOnlyPropertyDoesNotSupportPaths,
                targetProperty.Type,
                string.Join(".", propertyConfig.Target));
            return;
        }

        if (!PropertyPath.TryFind(
            ctx.Mapping.SourceType,
            propertyConfig.Source,
            out var sourcePropertyPath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.SourcePropertyNotFound,
                targetProperty.Name,
                ctx.Mapping.TargetType,
                ctx.Mapping.SourceType);
            return;
        }

        BuildInitPropertyMapping(ctx, targetProperty, sourcePropertyPath);
    }

    private static void BuildInitPropertyMapping(
        INewInstanceBuilderContext<IMapping> ctx,
        IPropertySymbol targetProperty,
        PropertyPath sourcePath)
    {
        var targetPath = new PropertyPath(new[]
        {
            targetProperty
        });
        if (!ObjectPropertyMappingBodyBuilder.ValidateMappingSpecification(ctx, sourcePath, targetPath, true))
            return;

        var delegateMapping = ctx.BuilderContext.FindMapping(sourcePath.MemberType, targetProperty.Type)
            ?? ctx.BuilderContext.FindOrBuildMapping(sourcePath.MemberType.NonNullable(), targetProperty.Type.NonNullable());

        if (delegateMapping == null)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotMapProperty,
                ctx.Mapping.SourceType,
                sourcePath.FullName,
                sourcePath.Member.Type,
                ctx.Mapping.TargetType,
                targetPath.FullName,
                targetPath.Member.Type);
            return;
        }

        if (delegateMapping.Equals(ctx.Mapping))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ReferenceLoopInInitOnlyMapping,
                ctx.Mapping.SourceType,
                sourcePath.FullName,
                ctx.Mapping.TargetType,
                targetPath.FullName);
            return;
        }

        var nullFallback = NullFallbackValue.Default;
        if (!delegateMapping.SourceType.IsNullable() && sourcePath.IsAnyNullable())
        {
            nullFallback = ctx.BuilderContext.GetNullFallbackValue(targetProperty.Type);
        }

        var propertyMapping = new NullPropertyMapping(
            delegateMapping,
            sourcePath,
            targetProperty.Type,
            nullFallback,
            !ctx.BuilderContext.IsExpression);
        var propertyAssignmentMapping = new PropertyAssignmentMapping(
            targetPath,
            propertyMapping);
        ctx.AddInitPropertyMapping(propertyAssignmentMapping);
    }

    private static void BuildConstructorMapping(INewInstanceBuilderContext<IMapping> ctx)
    {
        if (ctx.Mapping.TargetType is not INamedTypeSymbol namedTargetType)
        {
            ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
            return;
        }

        // attributed ctor is prio 1
        // parameterless ctor is prio 2
        // then by descending parameter count
        // ctors annotated with [Obsolete] are considered last unless they have a MapperConstructor attribute set
        var ctorCandidates = namedTargetType.Constructors
            .Where(ctor => ctor.IsAccessible())
            .OrderByDescending(x => x.HasAttribute(ctx.BuilderContext.Types.MapperConstructorAttribute))
            .ThenBy(x => x.HasAttribute(ctx.BuilderContext.Types.ObsoleteAttribute))
            .ThenByDescending(x => x.Parameters.Length == 0)
            .ThenByDescending(x => x.Parameters.Length);
        foreach (var ctorCandidate in ctorCandidates)
        {
            if (!TryBuildConstructorMapping(
                ctx,
                ctorCandidate,
                out var mappedTargetPropertyNames,
                out var constructorParameterMappings))
            {
                if (ctorCandidate.HasAttribute(ctx.BuilderContext.Types.MapperConstructorAttribute))
                {
                    ctx.BuilderContext.ReportDiagnostic(
                        DiagnosticDescriptors.CannotMapToConfiguredConstructor,
                        ctx.Mapping.SourceType,
                        ctorCandidate);
                }

                continue;
            }

            ctx.TargetProperties.RemoveRange(mappedTargetPropertyNames);
            foreach (var constructorParameterMapping in constructorParameterMappings)
            {
                ctx.AddConstructorParameterMapping(constructorParameterMapping);
            }

            return;
        }

        ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NoConstructorFound, ctx.BuilderContext.Target);
    }

    private static bool TryBuildConstructorMapping(
        INewInstanceBuilderContext<IMapping> ctx,
        IMethodSymbol ctor,
        [NotNullWhen(true)] out ISet<string>? mappedTargetPropertyNames,
        [NotNullWhen(true)] out ISet<ConstructorParameterMapping>? constructorParameterMappings)
    {
        constructorParameterMappings = new HashSet<ConstructorParameterMapping>();
        mappedTargetPropertyNames = new HashSet<string>();
        var skippedOptionalParam = false;
        foreach (var parameter in ctor.Parameters)
        {
            if (!TryFindConstructorParameterSourcePath(ctx, parameter, out var sourcePath))
            {
                // expressions do not allow skipping of optional parameters
                if (!parameter.IsOptional || ctx.BuilderContext.IsExpression)
                    return false;

                skippedOptionalParam = true;
                continue;
            }

            // nullability is handled inside the property mapping
            var paramType = parameter.Type.WithNullableAnnotation(parameter.NullableAnnotation);
            var delegateMapping = ctx.BuilderContext.FindMapping(sourcePath.MemberType, paramType)
                ?? ctx.BuilderContext.FindOrBuildMapping(sourcePath.Member.Type.NonNullable(), paramType.NonNullable());

            if (delegateMapping == null)
            {
                if (!parameter.IsOptional)
                    return false;

                skippedOptionalParam = true;
                continue;
            }

            if (delegateMapping.Equals(ctx.Mapping))
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.ReferenceLoopInCtorMapping,
                    ctx.Mapping.SourceType,
                    sourcePath.FullName,
                    ctx.Mapping.TargetType,
                    parameter.Name);
                return false;
            }

            var propertyMapping = new NullPropertyMapping(
                delegateMapping,
                sourcePath,
                paramType,
                ctx.BuilderContext.GetNullFallbackValue(paramType),
                !ctx.BuilderContext.IsExpression);
            var ctorMapping = new ConstructorParameterMapping(parameter, propertyMapping, skippedOptionalParam);
            constructorParameterMappings.Add(ctorMapping);
            mappedTargetPropertyNames.Add(parameter.Name);
        }

        return true;
    }

    private static bool TryFindConstructorParameterSourcePath(
        INewInstanceBuilderContext<IMapping> ctx,
        IParameterSymbol parameter,
        [NotNullWhen(true)] out PropertyPath? sourcePath)
    {
        sourcePath = null;

        if (!ctx.PropertyConfigsByRootTargetName.TryGetValue(parameter.Name, out var propertyConfigs))
        {
            return PropertyPath.TryFind(
                ctx.Mapping.SourceType,
                MemberPathCandidateBuilder.BuildMemberPathCandidates(parameter.Name),
                ctx.IgnoredSourcePropertyNames,
                StringComparer.OrdinalIgnoreCase,
                out sourcePath
            );
        }

        if (propertyConfigs.Count > 1)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForConstructorParameter,
                parameter.Type,
                parameter.Name);
        }

        var propertyConfig = propertyConfigs.First();
        if (propertyConfig.Target.Count > 1)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConstructorParameterDoesNotSupportPaths,
                parameter.Type,
                string.Join(".", propertyConfig.Target));
            return false;
        }

        if (!PropertyPath.TryFind(
            ctx.Mapping.SourceType,
            propertyConfig.Source,
            out sourcePath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.SourcePropertyNotFound,
                propertyConfig.Source,
                ctx.Mapping.TargetType,
                ctx.Mapping.SourceType);
            return false;
        }

        return true;
    }
}
