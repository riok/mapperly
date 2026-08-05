using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.ExternalMappings;

internal static class ExternalMappingsExtractor
{
    public static IEnumerable<IUserMapping> ExtractExternalMappings(
        IEnumerable<UseStaticMapperConfiguration> assemblyScopedStaticMappers,
        SimpleMappingBuilderContext ctx,
        INamedTypeSymbol mapperSymbol
    )
    {
        return ExtractExternalStaticMappings(assemblyScopedStaticMappers, ctx)
            .Concat(ExtractExternalStaticMappings(ExtractStaticMappersFromAttributes(ctx, mapperSymbol), ctx))
            .Concat(ExtractExternalInstanceMappings(ctx, mapperSymbol));
    }

    private static IEnumerable<UseStaticMapperConfiguration> ExtractStaticMappersFromAttributes(
        SimpleMappingBuilderContext ctx,
        INamedTypeSymbol mapperSymbol
    )
    {
        return ctx
            .AttributeAccessor.Access<UseStaticMapperAttribute, UseStaticMapperConfiguration>(mapperSymbol)
            .Concat(ctx.AttributeAccessor.Access<UseStaticMapperAttribute<object>, UseStaticMapperConfiguration>(mapperSymbol));
    }

    private static IEnumerable<IUserMapping> ExtractExternalStaticMappings(
        IEnumerable<UseStaticMapperConfiguration> staticMappers,
        SimpleMappingBuilderContext ctx
    )
    {
        var staticExternalMappers = staticMappers.SelectMany(x =>
            UserMethodMappingExtractor.ExtractUserImplementedMappings(
                ctx,
                x.MapperType,
                receiver: x.MapperType.FullyQualifiedIdentifierName(),
                isStatic: true,
                isExternal: true
            )
        );
        return staticExternalMappers;
    }

    private static IEnumerable<IUserMapping> ExtractExternalInstanceMappings(SimpleMappingBuilderContext ctx, INamedTypeSymbol mapperSymbol)
    {
        return ctx
            .SymbolAccessor.GetAllMembers(mapperSymbol)
            .Where(x => ctx.AttributeAccessor.HasAttribute<UseMapperAttribute>(x))
            .SelectMany(x => ValidateAndExtractExternalInstanceMappings(ctx, x));
    }

    public static IEnumerable<(string Name, IUserMapping Mapping)> ExtractExternalNamedMappings(
        SimpleMappingBuilderContext ctx,
        INamedTypeSymbol mapperSymbol
    )
    {
        var directRefs = ctx
            .SymbolAccessor.GetAllMethods(mapperSymbol)
            .SelectMany(CollectMemberMappingConfigurations)
            .SelectMany(e => UserMethodMappingExtractor.ExtractNamedUserImplementedMappings(ctx, e).Select(y => (e.FullName, y)));

        var transitiveUseRefs = ctx
            .SymbolAccessor.GetAllMethods(mapperSymbol)
            .SelectMany(x => CollectTransitiveUseRefs(ctx, x))
            .SelectMany(e => UserMethodMappingExtractor.ExtractNamedUserImplementedMappings(ctx, e).Select(y => (e.FullName, y)));

        // Overlap between direct and transitive refs is fine: the mapping collection dedupes the
        // same method rediscovered under the same name (callers pass ignoreDuplicates: true), and
        // genuinely conflicting names still surface via the existing ambiguous-name diagnostic (RMG060).
        return directRefs.Concat(transitiveUseRefs);

        IEnumerable<IMethodReferenceConfiguration> CollectMemberMappingConfigurations(IMethodSymbol x) =>
            ctx
                .AttributeAccessor.Access<MapPropertyAttribute, MemberMappingConfiguration>(x)
                .Select(e => e.Use)
                .Concat(ctx.AttributeAccessor.Access<MapPropertyFromSourceAttribute, MemberMappingConfiguration>(x).Select(e => e.Use))
                .Concat(
                    ctx.AttributeAccessor.Access<IncludeMappingConfigurationAttribute, IncludeMappingConfiguration>(x).Select(e => e.Name)
                )
                .Where(e => e?.IsExternal ?? false)
                .WhereNotNull();
    }

    /// <summary>
    /// For each IncludeMappingConfiguration referencing an external method,
    /// recursively collect:
    /// 1. Non-external Use references from that method's MapProperty/MapValue attributes
    /// 2. Nested IncludeMappingConfiguration targets (registered as named mappings)
    /// This ensures all transitively referenced named mappings are discoverable.
    /// </summary>
    private static IEnumerable<IMethodReferenceConfiguration> CollectTransitiveUseRefs(
        SimpleMappingBuilderContext ctx,
        IMethodSymbol method
    )
    {
        var includes = ctx
            .AttributeAccessor.Access<IncludeMappingConfigurationAttribute, IncludeMappingConfiguration>(method)
            .Select(e => e.Name)
            .Where(e => e is { IsExternal: true })
            .WhereNotNull()
            .ToList();

        var visited = new HashSet<string>();
        return Collect(includes);

        IEnumerable<IMethodReferenceConfiguration> Collect(IReadOnlyCollection<IMethodReferenceConfiguration> includeRefs)
        {
            foreach (var includeRef in includeRefs)
            {
                if (!visited.Add(includeRef.FullName))
                    continue;

                var targetType = includeRef.GetTargetType(ctx);
                if (targetType is null)
                    continue;

                var methods = ctx
                    .SymbolAccessor.GetAllMethods(targetType)
                    .Where(m => ctx.AttributeAccessor.IsMappingNameEqualTo(m, includeRef.Name));

                foreach (var m in methods)
                {
                    // Collect local Use refs from MapProperty/MapPropertyFromSource/MapValue
                    var useRefs = ctx
                        .AttributeAccessor.Access<MapPropertyAttribute, MemberMappingConfiguration>(m)
                        .Select(e => e.Use)
                        .Concat(
                            ctx.AttributeAccessor.Access<MapPropertyFromSourceAttribute, MemberMappingConfiguration>(m).Select(e => e.Use)
                        )
                        .Concat(ctx.AttributeAccessor.Access<MapValueAttribute, MemberValueMappingConfiguration>(m).Select(e => e.Use))
                        .Where(e => e is { IsExternal: false })
                        .WhereNotNull();

                    foreach (var localRef in useRefs)
                    {
                        yield return new IncludedLocalUseMethodReference(localRef.Name, includeRef);
                    }

                    // Follow nested IncludeMappingConfiguration attributes recursively
                    var nestedIncludes = ctx
                        .AttributeAccessor.Access<IncludeMappingConfigurationAttribute, IncludeMappingConfiguration>(m)
                        .Select(e => e.Name)
                        .Where(e => e is { IsExternal: true })
                        .WhereNotNull()
                        .ToList();

                    foreach (var nested in nestedIncludes)
                    {
                        yield return nested; // Register nested target as named mapping
                    }

                    foreach (var transitive in Collect(nestedIncludes))
                    {
                        yield return transitive;
                    }
                }
            }
        }
    }

    /// <summary>
    /// A method reference for a Use target that is local to an included external method.
    /// Registered with the simple method name (matching the lookup key) but resolves against the foreign type.
    /// Delegates receiver resolution to the include reference that discovered it.
    /// </summary>
    private record IncludedLocalUseMethodReference(string Name, IMethodReferenceConfiguration IncludeRef) : IMethodReferenceConfiguration
    {
        public string FullName => Name;
        public bool IsExternal => true;

        public INamedTypeSymbol? GetTargetType(SimpleMappingBuilderContext ctx) => IncludeRef.GetTargetType(ctx);

        public string? GetTargetName(SimpleMappingBuilderContext ctx) => IncludeRef.GetTargetName(ctx);
    }

    private static IEnumerable<IUserMapping> ValidateAndExtractExternalInstanceMappings(SimpleMappingBuilderContext ctx, ISymbol symbol)
    {
        var (name, type, nullableAnnotation) = symbol switch
        {
            IFieldSymbol field => (field.Name, field.Type, field.NullableAnnotation),
            IPropertySymbol prop => (prop.Name, prop.Type, prop.NullableAnnotation),
            _ => (string.Empty, null, NullableAnnotation.None),
        };

        if (type == null)
            return [];

        if (nullableAnnotation != NullableAnnotation.Annotated)
            return UserMethodMappingExtractor.ExtractUserImplementedMappings(ctx, type, name, isStatic: false, isExternal: true);

        ctx.ReportDiagnostic(DiagnosticDescriptors.ExternalMapperMemberCannotBeNullable, symbol, symbol.ToDisplayString());
        return [];
    }
}
