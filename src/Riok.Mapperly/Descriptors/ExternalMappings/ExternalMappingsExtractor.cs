using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.ExternalMappings;

internal static class ExternalMappingsExtractor
{
    public static IEnumerable<IUserMapping> ExtractExternalMappings(SimpleMappingBuilderContext ctx, INamedTypeSymbol mapperSymbol)
    {
        var staticExternalMappers = ctx
            .AttributeAccessor.ReadUseStaticMapperAttributes(mapperSymbol)
            .Concat(ctx.AttributeAccessor.ReadGenericUseStaticMapperAttributes(mapperSymbol))
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
            .Where(x => ctx.AttributeAccessor.HasUseMapperAttribute(x))
            .SelectMany(x => ValidateAndExtractExternalInstanceMappings(ctx, x));

        return staticExternalMappers.Concat(externalInstanceMappers);
    }

    public static IEnumerable<(string Name, IUserMapping Mapping)> ExtractExternalNamedMappings(
        SimpleMappingBuilderContext ctx,
        INamedTypeSymbol mapperSymbol
    )
    {
        return ctx
            .SymbolAccessor.GetAllMethods(mapperSymbol)
            .SelectMany(CollectMemberMappingConfigurations)
            .SelectMany(e => UserMethodMappingExtractor.ExtractNamedUserImplementedMappings(ctx, e).Select(y => (e.FullName, y)));

        IEnumerable<IMethodReferenceConfiguration> CollectMemberMappingConfigurations(IMethodSymbol x) =>
            ctx
                .AttributeAccessor.ReadMapPropertyAttributes(x)
                .Select(e => e.Use)
                .Concat(ctx.AttributeAccessor.ReadMapPropertyFromSourceAttributes(x).Select(e => e.Use))
                .Concat(ctx.AttributeAccessor.ReadIncludeMappingConfigurationAttributes(x).Select(e => e.Name))
                .Where(e => e?.IsExternal ?? false)
                .WhereNotNull();
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
