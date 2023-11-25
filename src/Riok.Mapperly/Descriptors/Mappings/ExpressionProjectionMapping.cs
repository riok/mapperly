using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A expression that can be used to construct
/// to map from one generic <see cref="IQueryable{T}"/> to another.
/// </summary>
public class ExpressionProjectionMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping delegateMapping)
    : MethodMapping(sourceType, targetType, null)
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // disable nullable reference types for expressions, as for ORMs nullables usually don't apply
        // #nullable disable
        // return x => ...;
        // #nullable enable
        var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();

        var delegateMappingSyntax = delegateMapping.Build(lambdaCtx);
        var projectionLambda = Lambda(lambdaSourceName, delegateMappingSyntax);
        var returnStatement = ctx.SyntaxFactory.Return(projectionLambda);
        return
        [
            returnStatement
                .WithLeadingTrivia(returnStatement.GetLeadingTrivia().Insert(0, ElasticCarriageReturnLineFeed).Insert(1, Nullable(false)))
                .WithTrailingTrivia(returnStatement.GetTrailingTrivia().Insert(0, ElasticCarriageReturnLineFeed).Insert(1, Nullable(true)))
        ];
    }
}
