using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A projections queryable mapping
/// to map from one generic <see cref="IQueryable{T}"/> to another.
/// </summary>
public class QueryableProjectionMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    INewInstanceMapping delegateMapping,
    bool supportsNullableAttributes
) : NewInstanceMethodMapping(sourceType, targetType)
{
    private const string QueryableReceiverName = "global::System.Linq.Queryable";
    private const string SelectMethodName = nameof(Queryable.Select);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        ctx.NameBuilder.Reserve(UsedParameterHelpers.ExtractUsedParameters(delegateMapping));

        // disable nullable reference types for expressions, as for ORMs nullables usually don't apply
        // #nullable disable
        // return System.Linq.Enumerable.Select(source, x => ...);
        // #nullable enable
        var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();

        var delegateMappingSyntax = delegateMapping.Build(lambdaCtx);
        var projectionLambda = Lambda(lambdaSourceName, delegateMappingSyntax);
        var select = ctx.SyntaxFactory.StaticInvocation(QueryableReceiverName, SelectMethodName, ctx.Source, projectionLambda);
        var returnStatement = ctx.SyntaxFactory.Return(select);
        var leadingTrivia = returnStatement.GetLeadingTrivia().Insert(0, ElasticCarriageReturnLineFeed).Insert(1, Nullable(false));
        var trailingTrivia = returnStatement
            .GetTrailingTrivia()
            .Insert(0, ElasticCarriageReturnLineFeed)
            .Insert(1, Nullable(true, !supportsNullableAttributes));
        returnStatement = returnStatement.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
        return [returnStatement];
    }
}
