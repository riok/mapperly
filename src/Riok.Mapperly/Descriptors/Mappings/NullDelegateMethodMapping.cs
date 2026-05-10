using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Null aware delegate mapping for <see cref="MethodMapping"/>s.
/// Abstracts handling null values of the delegated mapping.
/// </summary>
public class NullDelegateMethodMapping(
    ITypeSymbol nullableSourceType,
    ITypeSymbol nullableTargetType,
    MethodMapping delegateMapping,
    NullFallbackValue nullFallbackValue,
    bool nullableAttributesSupported
) : NewInstanceMethodMapping(nullableSourceType, nullableTargetType)
{
    public override IEnumerable<TypeMappingKey> BuildAdditionalMappingKeys(TypeMappingConfiguration config)
    {
        // if the fallback value is not nullable,
        // this mapping never returns null.
        // add the following mapping keys:
        // null => null (added by default)
        // null => non-null
        // non-null => non-null
        if (!nullFallbackValue.IsNullable(TargetType))
        {
            yield return new TypeMappingKey(SourceType, TargetType.NonNullable(), config);
            yield return new TypeMappingKey(SourceType.NonNullable(), TargetType.NonNullable(), config);
            yield break;
        }

        // this mapping never returns null for non-null input values
        // and is guarded with [return: NotNullIfNotNull]
        // therefore this mapping can also be used as mapping for non-null values.
        yield return new TypeMappingKey(delegateMapping, config);
    }

    protected internal override SyntaxList<AttributeListSyntax> BuildAttributes(TypeMappingBuildContext ctx)
    {
        return !nullableAttributesSupported || !TargetType.IsNullable() || !nullFallbackValue.IsNullable(TargetType)
            ? base.BuildAttributes(ctx)
            : base.BuildAttributes(ctx).Add(ctx.SyntaxFactory.ReturnNotNullIfNotNullAttribute(ctx.Source));
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var body = delegateMapping.BuildBody(ctx);
        return AddPreNullHandling(ctx, body);
    }

    private IEnumerable<StatementSyntax> AddPreNullHandling(TypeMappingBuildContext ctx, IEnumerable<StatementSyntax> body)
    {
        if (!SourceType.IsNullable() || delegateMapping.SourceType.IsNullable())
            return body;

        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        // if (source == null)
        //   return <null-substitute>;

        // Also include nullable [MapAdditionalSource] parameters in the null check.
        var nullableAdditionalSources = GetNullableAdditionalSourceExpressions(ctx);
        if (nullableAdditionalSources.Count > 0)
        {
            var returnExpression = NullSubstitute(TargetType, ctx.Source, nullFallbackValue);
            var conditions = new List<ExpressionSyntax> { IsNull(ctx.Source) };
            conditions.AddRange(nullableAdditionalSources);
            var combined = Or(conditions);
            var ifStatement = ctx.SyntaxFactory.If(combined, ctx.SyntaxFactory.AddIndentation().Return(returnExpression));
            return body.Prepend(ifStatement);
        }

        var fallbackExpression = NullSubstitute(TargetType, ctx.Source, nullFallbackValue);
        var ifExpression = ctx.SyntaxFactory.IfNullReturnOrThrow(ctx.Source, fallbackExpression);
        return body.Prepend(ifExpression);
    }

    private List<ExpressionSyntax> GetNullableAdditionalSourceExpressions(TypeMappingBuildContext ctx)
    {
        var result = new List<ExpressionSyntax>();

        foreach (var parameter in AdditionalSourceMergeParameters)
        {
            if (!parameter.Type.IsNullable())
                continue;

            result.Add(
                ctx.AdditionalSources?.TryGetValue(parameter.Type.ToDisplayString(), out var sourceExpr) == true
                    ? IsNull(sourceExpr)
                    : IsNull(IdentifierName(parameter.Name))
            );
        }

        return result;
    }
}
