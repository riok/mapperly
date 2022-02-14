using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.TypeMappings;
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

        return new NewInstanceObjectPropertyMapping(ctx.Source, ctx.Target);
    }

    public static void BuildMappingBody(MappingBuilderContext ctx, ObjectPropertyMapping mapping)
    {
        var ignoredTargetProperties = ctx.ListConfiguration<MapperIgnoreAttribute>()
            .Select(x => x.Target)
            .ToHashSet();

        var nameMappings = ctx.ListConfiguration<MapPropertyAttribute>()
            .ToDictionary(x => x.Target, x => x.Source);

        var targetProperties = mapping.TargetType
            .GetAllMembers()
            .OfType<IPropertySymbol>()
            .DistinctBy(x => x.Name);

        foreach (var targetProperty in targetProperties)
        {
            if (ignoredTargetProperties.Remove(targetProperty.Name))
                continue;

            var mappingNameWasManuallyConfigured = nameMappings.Remove(targetProperty.Name, out var sourcePropertyName);
            sourcePropertyName ??= targetProperty.Name;

            var sourceProperty = FindSourceProperty(mapping.SourceType, sourcePropertyName);
            if (sourceProperty != null)
            {
                if (BuildPropertyMapping(ctx, mapping, sourceProperty, targetProperty) is { } propertyMapping)
                {
                    mapping.AddPropertyMapping(propertyMapping);
                }
                continue;
            }

            if (mappingNameWasManuallyConfigured)
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.ConfiguredMappingSourcePropertyNotFound,
                    sourcePropertyName,
                    mapping.TargetType);
            }
        }

        AddUnmatchedIgnoredPropertiesDiagnostics(ctx, ignoredTargetProperties, mapping);
        AddUnmatchedTargetPropertiesDiagnostics(ctx, nameMappings.Keys, mapping);
    }

    private static void AddUnmatchedTargetPropertiesDiagnostics(
        MappingBuilderContext ctx,
        IEnumerable<string> propertyNames,
        ObjectPropertyMapping mapping)
    {
        foreach (var propertyName in propertyNames)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetPropertyNotFound,
                propertyName,
                mapping.TargetType);
        }
    }

    private static void AddUnmatchedIgnoredPropertiesDiagnostics(
        MappingBuilderContext ctx,
        HashSet<string> ignoredTargetProperties,
        ObjectPropertyMapping mapping)
    {
        foreach (var notFoundIgnoredProperty in ignoredTargetProperties)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredPropertyNotFound,
                notFoundIgnoredProperty,
                mapping.TargetType);
        }
    }

    private static IPropertySymbol? FindSourceProperty(ITypeSymbol source, string name)
    {
        return source.GetAllMembers(name)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => !p.IsStatic);
    }

    private static PropertyMappingDescriptor? BuildPropertyMapping(
        MappingBuilderContext ctx,
        ObjectPropertyMapping mapping,
        IPropertySymbol sourceProperty,
        IPropertySymbol targetProperty)
    {
        if (targetProperty.IsReadOnly)
            return null;

        if (sourceProperty.IsWriteOnly)
            return null;

        var propertyMapping = ctx.FindOrBuildMapping(sourceProperty.Type.NonNullable(), targetProperty.Type.NonNullable());
        if (propertyMapping == null)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.CouldNotMapProperty,
                mapping.SourceType,
                sourceProperty.Name,
                sourceProperty.Type,
                mapping.TargetType,
                targetProperty.Name,
                targetProperty.Type);
            return null;
        }

        var nullDelegateMapping = new NullDelegateMapping(
            sourceProperty.IsNullable(),
            targetProperty.IsNullable(),
            sourceProperty.Type,
            targetProperty.Type,
            propertyMapping);
        return new PropertyMappingDescriptor(sourceProperty, targetProperty, nullDelegateMapping);
    }
}
