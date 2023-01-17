using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBuilder;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilder;

public static class ObjectPropertyMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, ObjectPropertyMapping mapping)
    {
        var mappingCtx = new ObjectPropertyMappingBuilderContext<ObjectPropertyMapping>(ctx, mapping);
        BuildMappingBody(mappingCtx);
    }

    public static void BuildMappingBody(ObjectPropertyMappingBuilderContext ctx)
    {
        var propertyNameComparer = ctx.BuilderContext.MapperConfiguration.PropertyNameMappingStrategy ==
                       PropertyNameMappingStrategy.CaseSensitive
            ? StringComparer.Ordinal
            : StringComparer.OrdinalIgnoreCase;

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
                ctx.IgnoredSourcePropertyNames,
                propertyNameComparer,
                out var sourcePropertyPath))
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.MappingSourcePropertyNotFound,
                    targetProperty.Name,
                    ctx.Mapping.SourceType);
                continue;
            }

            if (targetProperty.IsReadOnly)
            {
                BuildPropertyReferenceMapping(ctx, sourcePropertyPath, new PropertyPath(new[] { targetProperty }));
            }
            else
            {
                BuildPropertyAssignmentMapping(ctx, sourcePropertyPath, new PropertyPath(new[] { targetProperty }));
            }
        }

        ctx.AddDiagnostics();
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

    public static bool ValidatePropertyReferenceMappingSpecification(
        ObjectPropertyMappingBuilderContext ctx,
        PropertyPath sourcePropertyPath,
        PropertyPath targetPropertyPath,
        bool allowInitOnlyMember = false)
    {
        if (targetPropertyPath.MemberType.IsValueType ||
            SymbolEqualityComparer.Default.Equals(targetPropertyPath.MemberType, ctx.BuilderContext.Types.String) ||
           (targetPropertyPath.MemberType.IsReferenceType && targetPropertyPath.Member.IsReadOnly && targetPropertyPath.Member.GetMethod?.IsAccessible() == false))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToReadOnlyValueProperty,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return false;
        }

        // a target property path part is write only or not accessible
        if (targetPropertyPath.ObjectPath.Any(p => p.IsWriteOnly || p.GetMethod?.IsAccessible() != true))
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

        // a source property path is write only or not accessible
        if (sourcePropertyPath.Path.Any(p => p.IsWriteOnly || p.GetMethod?.IsAccessible() != true))
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

    public static bool ValidateAssignmentMappingSpecification(
        ObjectPropertyMappingBuilderContext ctx,
        PropertyPath sourcePropertyPath,
        PropertyPath targetPropertyPath,
        bool allowInitOnlyMember = false)
    {
        // Target property cannot be the following
        // must be assignable
        if (targetPropertyPath.Member.IsReadOnly || targetPropertyPath.Member.SetMethod?.IsAccessible() == false)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapToReadOnlyValueProperty,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return false;
        }

        // a target property path part is write only or not accessible
        if (targetPropertyPath.ObjectPath.Any(p => p.IsWriteOnly || p.GetMethod?.IsAccessible() != true))
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

        // a source property path is write only or not accessible
        if (sourcePropertyPath.Path.Any(p => p.IsWriteOnly || p.GetMethod?.IsAccessible() != true))
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
        if (!ValidateAssignmentMappingSpecification(ctx, sourcePropertyPath, targetPropertyPath))
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
        if (delegateMapping.SourceType.IsNullable() || delegateMapping.IsSynthetic && targetPropertyPath.Member.IsNullable())
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

    private static void BuildPropertyReferenceMapping(
        ObjectPropertyMappingBuilderContext ctx,
        PropertyPath sourcePropertyPath,
        PropertyPath targetPropertyPath)
    {
        if (!ValidatePropertyReferenceMappingSpecification(ctx, sourcePropertyPath, targetPropertyPath))
            return;

        // nullability is handled inside the property mapping
        // since we check mapping on a type basis, readonly properties do not count as readonly types
        // but we still need someway of differentiating readonly mappings from normal mappings, so we add
        // the refkind as an extra key
        var delegateMapping = ctx.BuilderContext.FindMapping(sourcePropertyPath.Member.Type, targetPropertyPath.Member.Type, RefKind.Ref) ??
            ctx.BuilderContext.FindOrBuildMapping(
                sourcePropertyPath.Member.Type.NonNullable(),
                targetPropertyPath.Member.Type.NonNullable(),
                RefKind.Ref);

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

            ctx.AddPropertyReferenceMapping(new PropertyReferenceMapping(targetPropertyPath, propertyMapping));
            return;
        }
    }
}
