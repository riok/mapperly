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
        var staticExternalMappers = ctx.AttributeAccessor.Access<UseStaticMapperAttribute, UseStaticMapperConfiguration>(mapperSymbol)
            .Concat(ctx.AttributeAccessor.Access<UseStaticMapperAttribute<object>, UseStaticMapperConfiguration>(mapperSymbol))
            .SelectMany(x =>
                UserMethodMappingExtractor.ExtractUserImplementedMappings(
                    ctx,
                    x.MapperType,
                    x.MapperType.FullyQualifiedIdentifierName(),
                    true
                )
            );

        var externalInstanceMappers = ctx.SymbolAccessor.GetAllMembers(mapperSymbol)
            .Where(x => ctx.AttributeAccessor.HasAttribute<UseMapperAttribute>(x))
            .SelectMany(x => ValidateAndExtractExternalInstanceMappings(ctx, x));

        return staticExternalMappers.Concat(externalInstanceMappers);
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
            return Enumerable.Empty<IUserMapping>();

        if (nullableAnnotation != NullableAnnotation.Annotated)
            return UserMethodMappingExtractor.ExtractUserImplementedMappings(ctx, type, name, false);

        ctx.ReportDiagnostic(DiagnosticDescriptors.ExternalMapperMemberCannotBeNullable, symbol, symbol.ToDisplayString());
        return Enumerable.Empty<IUserMapping>();
    }
}
