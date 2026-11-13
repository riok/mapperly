using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.ExternalMappings;

internal sealed class ExternalMappingsExtractor
{
    private readonly SimpleMappingBuilderContext _ctx;
    private readonly INamedTypeSymbol _mapperSymbol;
    private readonly IReadOnlyCollection<UseStaticMapperConfiguration> _staticMappers;

    public ExternalMappingsExtractor(SimpleMappingBuilderContext ctx, IEnumerable<UseStaticMapperConfiguration> assemblyScopedStaticMappers)
    {
        _ctx = ctx;
        _mapperSymbol = ctx.MapperDeclaration.Symbol;
        _staticMappers = assemblyScopedStaticMappers.Concat(ExtractStaticMappersFromAttributes()).ToArray();
    }

    public IEnumerable<IUserMapping> ExtractExternalMappings()
    {
        return ExtractExternalStaticMappings().Concat(ExtractExternalInstanceMappings());
    }

    private IEnumerable<UseStaticMapperConfiguration> ExtractStaticMappersFromAttributes()
    {
        return _ctx
            .AttributeAccessor.Access<UseStaticMapperAttribute, UseStaticMapperConfiguration>(_mapperSymbol)
            .Concat(_ctx.AttributeAccessor.Access<UseStaticMapperAttribute<object>, UseStaticMapperConfiguration>(_mapperSymbol));
    }

    private IEnumerable<IUserMapping> ExtractExternalStaticMappings()
    {
        var staticExternalMappers = _staticMappers.SelectMany(x =>
            UserMethodMappingExtractor.ExtractUserImplementedMappings(
                _ctx,
                x.MapperType,
                receiver: x.MapperType.FullyQualifiedIdentifierName(),
                isStatic: true,
                isExternal: true,
                noExpressionInlining: x.NoExpressionInlining
            )
        );
        return staticExternalMappers;
    }

    private IEnumerable<IUserMapping> ExtractExternalInstanceMappings()
    {
        return _ctx
            .SymbolAccessor.GetAllMembers(_mapperSymbol)
            .Where(x => _ctx.AttributeAccessor.HasAttribute<UseMapperAttribute>(x))
            .SelectMany(ValidateAndExtractExternalInstanceMappings);
    }

    public IEnumerable<(string Name, IUserMapping Mapping)> ExtractExternalNamedMappings()
    {
        return _ctx
            .SymbolAccessor.GetAllMethods(_mapperSymbol)
            .SelectMany(CollectMemberMappingConfigurations)
            .SelectMany(e =>
                UserMethodMappingExtractor
                    .ExtractNamedUserImplementedMappings(_ctx, e, IsNoExpressionInlining(e))
                    .Select(y => (e.FullName, y))
            );

        IEnumerable<IMethodReferenceConfiguration> CollectMemberMappingConfigurations(IMethodSymbol x) =>
            _ctx
                .AttributeAccessor.Access<MapPropertyAttribute, MemberMappingConfiguration>(x)
                .Select(e => e.Use)
                .Concat(_ctx.AttributeAccessor.Access<MapPropertyFromSourceAttribute, MemberMappingConfiguration>(x).Select(e => e.Use))
                .Concat(
                    _ctx.AttributeAccessor.Access<IncludeMappingConfigurationAttribute, IncludeMappingConfiguration>(x).Select(e => e.Name)
                )
                .Where(e => e?.IsExternal ?? false)
                .WhereNotNull();
    }

    private IEnumerable<IUserMapping> ValidateAndExtractExternalInstanceMappings(ISymbol symbol)
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
        {
            var noExpressionInlining = _ctx.AttributeAccessor.AccessSingle<UseMapperAttribute>(symbol).NoExpressionInlining;
            return UserMethodMappingExtractor.ExtractUserImplementedMappings(
                _ctx,
                type,
                name,
                isStatic: false,
                isExternal: true,
                noExpressionInlining
            );
        }

        _ctx.ReportDiagnostic(DiagnosticDescriptors.ExternalMapperMemberCannotBeNullable, symbol, symbol.ToDisplayString());
        return [];
    }

    private bool IsNoExpressionInlining(IMethodReferenceConfiguration target)
    {
        return target switch
        {
            ExternalInstanceMethodReferenceConfiguration instance => _ctx.AttributeAccessor.AccessFirstOrDefault<UseMapperAttribute>(
                instance.TargetMember
            )?.NoExpressionInlining
                ?? false,
            ExternalStaticMethodReferenceConfiguration staticTarget => _staticMappers.Any(x =>
                x.NoExpressionInlining && SymbolEqualityComparer.Default.Equals(x.MapperType, staticTarget.TargetType)
            ),
            _ => false,
        };
    }
}
