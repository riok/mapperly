using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.ExternalMappings;

internal static class ExternalMappingsExtractor
{
    public static IEnumerable<IUserMapping> ExtractExternalMappings(SimpleMappingBuilderContext ctx, INamedTypeSymbol mapperSymbol)
    {
        var staticExternalMappers = ctx
            .AttributeAccessor.Access<UseStaticMapperAttribute, UseStaticMapperConfiguration>(mapperSymbol)
            .Concat(ctx.AttributeAccessor.Access<UseStaticMapperAttribute<object>, UseStaticMapperConfiguration>(mapperSymbol))
            .SelectMany(x =>
                UserMethodMappingExtractor.ExtractUserImplementedMappings(
                    ctx,
                    x.MapperType,
                    receiver: x.MapperType.FullyQualifiedIdentifierName(),
                    isStatic: true,
                    isExternal: true
                )
            );

        var externalInstanceMappers = ctx
            .SymbolAccessor.GetAllMembers(mapperSymbol)
            .Where(x => ctx.AttributeAccessor.HasAttribute<UseMapperAttribute>(x))
            .SelectMany(x => ValidateAndExtractExternalInstanceMappings(ctx, x));

        return staticExternalMappers.Concat(externalInstanceMappers);
    }

    public static IEnumerable<(string Name, IUserMapping Mapping)> ExtractExternalNamedMappings(
        SimpleMappingBuilderContext ctx,
        INamedTypeSymbol mapperSymbol
    )
    {
        var externalStaticDirectlyReferencedMappings = ctx
            .SymbolAccessor.GetAllMethods(mapperSymbol)
            .SelectMany(CollectMemberMappingConfigurations)
            .SelectMany(e => UserMethodMappingExtractor.ExtractNamedUserImplementedMappings(ctx, e).Select(y => (e.FullName, y)));

        return externalStaticDirectlyReferencedMappings;

        IEnumerable<MethodReferenceConfiguration> CollectMemberMappingConfigurations(IMethodSymbol x) =>
            ctx
                .AttributeAccessor.Access<MapPropertyAttribute, MemberMappingConfiguration>(x)
                .Select(e => e.Use)
                .Concat(ctx.AttributeAccessor.Access<MapPropertyFromSourceAttribute, MemberMappingConfiguration>(x).Select(e => e.Use))
                .Concat(
                    ctx.AttributeAccessor.Access<IncludeMappingConfigurationAttribute, IncludeMappingConfiguration>(x).Select(e => e.Name)
                )
                .Where(e => e?.TargetType is not null)!;
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
