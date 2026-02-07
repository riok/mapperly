using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An expression mapping that returns an <see cref="Expression{TDelegate}"/>
/// where TDelegate is a <see cref="Func{TSource, TTarget}"/>.
/// </summary>
public class ExpressionMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    INewInstanceMapping delegateMapping,
    bool supportsNullableAttributes
) : NewInstanceMethodMapping(sourceType, targetType)
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // disable nullable reference types for expressions, as for ORMs nullables usually don't apply
        // #nullable disable
        // return (TSource x) => new TTarget { ... };
        // #nullable enable
        var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();

        var delegateMappingSyntax = delegateMapping.Build(lambdaCtx);
        var projectionLambda = Lambda(lambdaSourceName, delegateMappingSyntax);
        var returnStatement = ctx.SyntaxFactory.Return(projectionLambda);
        var leadingTrivia = returnStatement.GetLeadingTrivia().Insert(0, ElasticCarriageReturnLineFeed).Insert(1, Nullable(false));
        var trailingTrivia = returnStatement
            .GetTrailingTrivia()
            .Insert(0, ElasticCarriageReturnLineFeed)
            .Insert(1, Nullable(true, !supportsNullableAttributes));
        returnStatement = returnStatement.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
        return [returnStatement];
    }
}
