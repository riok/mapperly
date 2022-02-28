using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class ObjectPropertyMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.Target.HasAccessibleParameterlessConstructor())
            return null;

        if (ctx.Target.SpecialType != SpecialType.None || ctx.Source.SpecialType != SpecialType.None)
            return null;

        return new NewInstanceObjectPropertyMapping(ctx.Source, ctx.Target.NonNullable());
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, ObjectPropertyMapping mapping)
    {
        var mappingCtx = new ObjectPropertyMappingBuilderContext(ctx, mapping);

        var ignoredTargetProperties = ctx.ListConfiguration<MapperIgnoreAttribute>()
            .Select(x => x.Target)
            .ToHashSet();

        var propertyConfigsByRootTargetName = ctx.ListConfiguration<MapPropertyAttribute>()
            .GroupBy(x => x.Target.First())
            .ToDictionary(x => x.Key, x => x.ToList());

        var targetProperties = mapping.TargetType
            .GetAllMembers()
            .OfType<IPropertySymbol>()
            .DistinctBy(x => x.Name);

        foreach (var targetProperty in targetProperties)
        {
            if (ignoredTargetProperties.Remove(targetProperty.Name))
                continue;

            if (propertyConfigsByRootTargetName.Remove(targetProperty.Name, out var propertyConfigs))
            {
                // add all configured mappings
                // order by target path count to map less nested items first (otherwise they would overwrite all others)
                // eg. target.A = source.B should be mapped before target.A.Id = source.B.Id
                foreach (var config in propertyConfigs.OrderBy(x => x.Target.Count))
                {
                    BuildPropertyMapping(mappingCtx, config.Source, config.Target, true);
                }

                continue;
            }

            // only try other namings if the property was not found,
            // ignore all other results
            var targetPropertyPath = new[] { targetProperty.Name };
            var targetPropFound = false;
            foreach (var sourcePropertyCandidate in MemberPathCandidateBuilder.BuildMemberPathCandidates(targetProperty.Name))
            {
                if (BuildPropertyMapping(mappingCtx, sourcePropertyCandidate.ToList(), targetPropertyPath) is not ValidationResult.PropertyNotFound)
                {
                    targetPropFound = true;
                    break;
                }
            }

            // target property couldn't be found
            // add a diagnostic.
            if (!targetPropFound)
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.MappingSourcePropertyNotFound,
                    targetProperty.Name,
                    mapping.SourceType);
            }
        }

        AddUnmatchedIgnoredPropertiesDiagnostics(mappingCtx, ignoredTargetProperties);
        AddUnmatchedTargetPropertiesDiagnostics(mappingCtx, propertyConfigsByRootTargetName.Values.SelectMany(x => x));
    }

    private static ValidationResult BuildPropertyMapping(
        ObjectPropertyMappingBuilderContext ctx,
        IReadOnlyCollection<string> sourcePath,
        IReadOnlyCollection<string> targetPath,
        bool configuredTargetPropertyPath = false)
    {
        var targetPropertyPath = new PropertyPath(FindPropertyPath(ctx.Mapping.TargetType, targetPath).ToList());
        var sourcePropertyPath = new PropertyPath(FindPropertyPath(ctx.Mapping.SourceType, sourcePath).ToList());

        var validationResult = ValidateMapping(
            ctx,
            sourcePropertyPath,
            targetPropertyPath,
            sourcePath,
            targetPath,
            configuredTargetPropertyPath);
        if (validationResult != ValidationResult.Ok)
            return validationResult;

        // nullability is handled inside the property mapping
        var delegateMapping = ctx.BuilderContext.FindMapping(sourcePropertyPath.Member.Type.UpgradeNullable(), targetPropertyPath.Member.Type.UpgradeNullable())
            ?? ctx.BuilderContext.FindOrBuildMapping(sourcePropertyPath.Member.Type.NonNullable(), targetPropertyPath.Member.Type.NonNullable());

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
            return ValidationResult.CannotMapTypes;
        }

        // no member of the source path is nullable, no null handling needed
        if (!sourcePropertyPath.IsAnyNullable())
        {
            ctx.AddPropertyMapping(new PropertyMapping(
                sourcePropertyPath,
                targetPropertyPath,
                delegateMapping,
                false));
            return ValidationResult.Ok;
        }

        // the source is nullable, or the mapping is a direct assignment and the target allows nulls
        // access the source in a null save matter (via ?.) but no other special handling required.
        if (delegateMapping.SourceType.IsNullable() || delegateMapping is DirectAssignmentMapping && targetPropertyPath.Member.IsNullable())
        {
            ctx.AddPropertyMapping(new PropertyMapping(
                sourcePropertyPath,
                targetPropertyPath,
                delegateMapping,
                true));
            return ValidationResult.Ok;
        }

        // additional null condition check
        // (only map if source is not null, else may throw depending on settings)
        ctx.AddNullDelegatePropertyMapping(new PropertyMapping(
            sourcePropertyPath,
            targetPropertyPath,
            delegateMapping,
            false));
        return ValidationResult.Ok;
    }

    private static ValidationResult ValidateMapping(
        ObjectPropertyMappingBuilderContext ctx,
        PropertyPath sourcePropertyPath,
        PropertyPath targetPropertyPath,
        IReadOnlyCollection<string> configuredSourcePropertyPath,
        IReadOnlyCollection<string> configuredTargetPropertyPath,
        bool reportDiagnosticIfPropertyNotFound)
    {
        // the path parts don't match, not all target properties could be found
        if (configuredTargetPropertyPath.Count != targetPropertyPath.Path.Count)
        {
            if (reportDiagnosticIfPropertyNotFound)
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.ConfiguredMappingTargetPropertyNotFound,
                    string.Join(PropertyPath.PropertyAccessSeparator, configuredTargetPropertyPath),
                    ctx.Mapping.TargetType);
            }
            return ValidationResult.PropertyNotFound;
        }

        // the path parts don't match, not all source properties could be found
        if (configuredSourcePropertyPath.Count != sourcePropertyPath.Path.Count)
        {
            if (reportDiagnosticIfPropertyNotFound)
            {
                ctx.BuilderContext.ReportDiagnostic(
                    DiagnosticDescriptors.ConfiguredMappingSourcePropertyNotFound,
                    string.Join(PropertyPath.PropertyAccessSeparator, configuredSourcePropertyPath),
                    ctx.Mapping.SourceType);
            }
            return ValidationResult.PropertyNotFound;
        }

        // the target property path is readonly
        if (targetPropertyPath.Member.IsReadOnly)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CanNotMapToReadOnlyProperty,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return ValidationResult.PropertyHasUnexpectedSpecification;
        }

        // a target property path part is write only
        if (targetPropertyPath.ObjectPath.Any(p => p.IsWriteOnly))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CanNotMapToWriteOnlyPropertyPath,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return ValidationResult.PropertyHasUnexpectedSpecification;
        }

        // a source property path is write only
        if (sourcePropertyPath.Path.Any(p => p.IsWriteOnly))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CanNotMapFromWriteOnlyProperty,
                ctx.Mapping.SourceType,
                sourcePropertyPath.FullName,
                sourcePropertyPath.Member.Type,
                ctx.Mapping.TargetType,
                targetPropertyPath.FullName,
                targetPropertyPath.Member.Type);
            return ValidationResult.PropertyHasUnexpectedSpecification;
        }

        return ValidationResult.Ok;
    }

    private static IEnumerable<IPropertySymbol> FindPropertyPath(ITypeSymbol type, IEnumerable<string> path)
    {
        foreach (var name in path)
        {
            if (FindProperty(type, name) is not { } property)
                break;

            type = property.Type;
            yield return property;
        }
    }

    private static IPropertySymbol? FindProperty(ITypeSymbol type, string name)
    {
        return type.GetAllMembers(name)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => !p.IsStatic);
    }

    private static void AddUnmatchedTargetPropertiesDiagnostics(
        ObjectPropertyMappingBuilderContext ctx,
        IEnumerable<MapPropertyAttribute> unmatchedConfiguredProperties)
    {
        foreach (var propertyConfig in unmatchedConfiguredProperties)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetPropertyNotFound,
                propertyConfig.TargetFullName,
                ctx.Mapping.TargetType);
        }
    }

    private static void AddUnmatchedIgnoredPropertiesDiagnostics(
        ObjectPropertyMappingBuilderContext ctx,
        HashSet<string> ignoredTargetProperties)
    {
        foreach (var notFoundIgnoredProperty in ignoredTargetProperties)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredPropertyNotFound,
                notFoundIgnoredProperty,
                ctx.Mapping.TargetType);
        }
    }

    private enum ValidationResult
    {
        Ok,
        PropertyNotFound,
        PropertyHasUnexpectedSpecification,
        CannotMapTypes,
    }
}
