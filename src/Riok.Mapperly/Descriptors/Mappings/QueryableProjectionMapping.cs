using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A projections queryable mapping
/// to map from one generic <see cref="IQueryable{T}"/> to another.
/// </summary>
public class QueryableProjectionMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping delegateMapping)
    : NewInstanceMethodMapping(sourceType, targetType)
{
    private const string QueryableReceiverName = "global::System.Linq.Queryable";
    private const string SelectMethodName = nameof(Queryable.Select);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // disable nullable reference type warnings for expressions, as for ORMs nullables usually don't apply
        // #nullable disable warnings
        // return global::System.Linq.Queryable.Select(source, x => ...);
        // #nullable enable warnings
        var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();

        var delegateMappingSyntax = delegateMapping.Build(lambdaCtx);
        var projectionLambda = Lambda(lambdaSourceName, delegateMappingSyntax);
        var select = ctx.SyntaxFactory.StaticInvocation(QueryableReceiverName, SelectMethodName, ctx.Source, projectionLambda);
        var returnStatement = ctx.SyntaxFactory.Return(select);
        return [ctx.SyntaxFactory.DisableNullableWarnings(returnStatement)];
    }
}
