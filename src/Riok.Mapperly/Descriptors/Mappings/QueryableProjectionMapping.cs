using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A projections queryable mapping
/// to map from one generic <see cref="IQueryable{T}"/> to another.
/// </summary>
public class QueryableProjectionMapping : MethodMapping
{
    private const string SelectLambdaParameterName = "x";

    private const string QueryableReceiverName = "System.Linq.Queryable";
    private const string SelectMethodName = nameof(Queryable.Select);

    private readonly ITypeMapping _delegateMapping;

    public QueryableProjectionMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping delegateMapping) : base(sourceType, targetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // disable nullable reference types for expressions, as for ORMs nullables usually don't apply
        // #nullable disable
        // return System.Linq.Enumerable.Select(source, x => ...);
        // #nullable enable
        var lambdaParamName = ctx.NameBuilder.New(SelectLambdaParameterName);
        var delegateMapping = _delegateMapping.Build(ctx.WithSource(IdentifierName(lambdaParamName)));
        var projectionLambda = SimpleLambdaExpression(Parameter(Identifier(lambdaParamName))).WithExpressionBody(delegateMapping);
        var select = StaticInvocation(QueryableReceiverName, SelectMethodName, ctx.Source, projectionLambda);
        return new[]
        {
            ReturnStatement(select)
                .WithLeadingTrivia(TriviaList(Nullable(false)))
                .WithTrailingTrivia(TriviaList(Nullable(true)))
        };
    }
}
