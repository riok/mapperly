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

        // Also check for nullable additional sources with MapAdditionalSource attribute
        var additionalSourceChecks = GetNullableAdditionalSourceChecks(ctx);
        if (additionalSourceChecks.Count > 0)
        {
            // For combined null checks, use null literal for Default fallback on nullable reference types
            // to generate idiomatic "return null;" instead of "return default;"
            var returnExpression = NullSubstitute(TargetType, ctx.Source, nullFallbackValue);
            var combinedCondition = CombineConditions(ctx.Source, additionalSourceChecks);
            var ifStatement = ctx.SyntaxFactory.If(combinedCondition, ctx.SyntaxFactory.AddIndentation().Return(returnExpression));
            return body.Prepend(ifStatement);
        }

        var fallbackExpression = NullSubstitute(TargetType, ctx.Source, nullFallbackValue);
        var ifExpression = ctx.SyntaxFactory.IfNullReturnOrThrow(ctx.Source, fallbackExpression);
        return body.Prepend(ifExpression);
    }

    private List<(ExpressionSyntax Condition, string ParameterName)> GetNullableAdditionalSourceChecks(TypeMappingBuildContext ctx)
    {
        var checks = new List<(ExpressionSyntax Condition, string ParameterName)>();

        // AdditionalSourceParameters are already filtered to only include parameters with MapAdditionalSource attribute
        foreach (var parameter in AdditionalSourceParameters)
        {
            // Check if parameter type is nullable
            if (!parameter.Type.IsNullable())
            {
                continue;
            }

            var parameterAccess = ctx.AdditionalSources?.TryGetValue(parameter.Name, out var source) is true
                ? source
                : IdentifierName(parameter.Name);

            var condition = IsNull(parameterAccess);
            checks.Add((condition, parameter.Name));
        }

        return checks;
    }

    private static ExpressionSyntax CombineConditions(
        ExpressionSyntax mainSource,
        List<(ExpressionSyntax Condition, string ParameterName)> additionalChecks
    )
    {
        if (additionalChecks.Count == 0)
            return IsNull(mainSource);

        var conditions = new List<ExpressionSyntax> { IsNull(mainSource) };
        foreach (var (_, parameterName) in additionalChecks)
        {
            conditions.Add(IsNull(IdentifierName(parameterName)));
        }

        return Or(conditions);
    }
}
