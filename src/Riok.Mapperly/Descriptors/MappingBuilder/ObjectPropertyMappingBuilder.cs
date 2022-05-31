using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class ObjectPropertyMappingBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, ObjectPropertyMapping mapping)
    {
        var mappingCtx = new ObjectPropertyMappingBuilderContext<ObjectPropertyMapping>(ctx, mapping);
        BuildMappingBody(mappingCtx);
    }

    public static void BuildMappingBody(ObjectPropertyMappingBuilderContext ctx)
    {
        foreach (var targetProperty in ctx.TargetProperties.Values)
        {
            if (ctx.PropertyConfigsByRootTargetName.Remove(targetProperty.Name, out var propertyConfigs))
            {
                // add all configured mappings
                // order by target path count to map less nested items first (otherwise they would overwrite all others)
                // eg. target.A = source.B should be mapped before target.A.Id = source.B.Id
                foreach (var config in propertyConfigs.OrderBy(x => x.Target.Count))
                {
                    BuildPropertyAssignmentMapping(ctx, config);
                }

                continue;
            }

            if (!PropertyPath.TryFind(
                ctx.Mapping.SourceType,
                MemberPathCandidateBuilder.BuildMemberPathCandidates(targetProperty.Name),
                out var sourcePropertyPath))
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.MappingSourcePropertyNotFound,
                    targetProperty.Name,
                    ctx.Mapping.SourceType);
                continue;
            }

            BuildPropertyAssignmentMapping(ctx, sourcePropertyPath, new PropertyPath(new[] { targetProperty }));
        }

        ctx.AddUnmatchedIgnoredPropertiesDiagnostics();
        ctx.AddUnmatchedTargetPropertiesDiagnostics();
        ctx.AddUnmatchedSourcePropertiesDiagnostics();
    }

    private static void BuildPropertyAssignmentMapping(ObjectPropertyMappingBuilderContext ctx, MapPropertyAttribute config)
    {
        if (!PropertyPath.TryFind(ctx.Mapping.TargetType, config.Target, out var targetPropertyPath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetPropertyNotFound,
                string.Join(PropertyPath.PropertyAccessSeparator, config.Target),
                ctx.Mapping.TargetType);
            return;
        }

        if (!PropertyPath.TryFind(ctx.Mapping.SourceType, config.Source, out var sourcePropertyPath))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingSourcePropertyNotFound,
                string.Join(PropertyPath.PropertyAccessSeparator, config.Source),
                ctx.Mapping.SourceType);
            return;
        }

        BuildPropertyAssignmentMapping(ctx, sourcePropertyPath, targetPropertyPath);
    }

    public static bool ValidateMappingSpecification(
        ObjectPropertyMappingBuilderContext ctx,
        PropertyPath sourcePropertyPath,
        PropertyPath targetPropertyPath,
        bool allowInitOnlyMember = false)
    {
        // the target property path is readonly
        if (targetPropertyPath.Member.IsReadOnly)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToReadOnlyProperty,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return false;
        }

        // a target property path part is write only
        if (targetPropertyPath.ObjectPath.Any(p => p.IsWriteOnly))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToWriteOnlyPropertyPath,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return false;
        }

        // a target property path part is init only
        var noInitOnlyPath = allowInitOnlyMember ? targetPropertyPath.ObjectPath : targetPropertyPath.Path;
        if (noInitOnlyPath.Any(p => p.IsInitOnly()))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToInitOnlyPropertyPath,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return false;
        }

        // a source property path is write only
        if (sourcePropertyPath.Path.Any(p => p.IsWriteOnly))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapFromWriteOnlyProperty,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return false;
        }

        return true;
    }

    private static void BuildPropertyAssignmentMapping(
        ObjectPropertyMappingBuilderContext ctx,
        PropertyPath sourcePropertyPath,
        PropertyPath targetPropertyPath)
    {
        if (!ValidateMappingSpecification(ctx, sourcePropertyPath, targetPropertyPath))
            return;

        // nullability is handled inside the property mapping
        var delegateMapping = ctx.BuilderContext.FindMapping(sourcePropertyPath.Member.Type, targetPropertyPath.Member.Type)
            ?? ctx.BuilderContext.FindOrBuildMapping(sourcePropertyPath.Member.Type.NonNullable(),
                targetPropertyPath.Member.Type.NonNullable());

        // couldn't build the mapping
        if (delegateMapping == null)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotMapProperty,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return;
        }

        // no member of the source path is nullable, no null handling needed
        if (!sourcePropertyPath.IsAnyNullable())
        {
            var propertyMapping = new PropertyMapping(
                delegateMapping,
                sourcePropertyPath,
                false,
                true);
            ctx.AddPropertyAssignmentMapping(new PropertyAssignmentMapping(targetPropertyPath, propertyMapping));
            return;
        }

        // the source is nullable, or the mapping is a direct assignment and the target allows nulls
        // access the source in a null save matter (via ?.) but no other special handling required.
        if (delegateMapping.SourceType.IsNullable() || delegateMapping is DirectAssignmentMapping && targetPropertyPath.Member.IsNullable())
        {
            var propertyMapping = new PropertyMapping(
                delegateMapping,
                sourcePropertyPath,
                true,
                false);
            ctx.AddPropertyAssignmentMapping(new PropertyAssignmentMapping(targetPropertyPath, propertyMapping));
            return;
        }

        // additional null condition check
        // (only map if source is not null, else may throw depending on settings)
        ctx.AddNullDelegatePropertyAssignmentMapping(new PropertyAssignmentMapping(
            targetPropertyPath,
            new PropertyMapping(delegateMapping, sourcePropertyPath, false, true)));
    }
}
