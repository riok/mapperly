using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A projections queryable mapping
/// to map from one generic <see cref="IQueryable{T}"/> to another.
/// </summary>
public class QueryableProjectionMapping : MethodMapping
{
    private const string QueryableReceiverName = "System.Linq.Queryable";
    private const string SelectMethodName = nameof(Queryable.Select);

    private readonly INewInstanceMapping _delegateMapping;

    public QueryableProjectionMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping delegateMapping)
        : base(sourceType, targetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // disable nullable reference types for expressions, as for ORMs nullables usually don't apply
        // #nullable disable
        // return System.Linq.Enumerable.Select(source, x => ...);
        // #nullable enable
        var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();

        var delegateMapping = _delegateMapping.Build(lambdaCtx);
        var projectionLambda = Lambda(lambdaSourceName, delegateMapping);
        var select = StaticInvocation(QueryableReceiverName, SelectMethodName, ctx.Source, projectionLambda);
        var returnStatement = ctx.SyntaxFactory.Return(select);
        return new[]
        {
            returnStatement
                .WithLeadingTrivia(returnStatement.GetLeadingTrivia().Insert(0, ElasticCarriageReturnLineFeed).Insert(1, Nullable(false)))
                .WithTrailingTrivia(returnStatement.GetTrailingTrivia().Insert(0, ElasticCarriageReturnLineFeed).Insert(1, Nullable(true)))
        };
    }
}
