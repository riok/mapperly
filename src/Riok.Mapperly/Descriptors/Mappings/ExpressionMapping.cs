using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An expression mapping that returns an <see cref="Expression{TDelegate}"/>
/// where TDelegate is a <see cref="Func{TSource, TTarget}"/>.
/// </summary>
public class ExpressionMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping delegateMapping)
    : NewInstanceMethodMapping(sourceType, targetType)
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // disable nullable reference type warnings for expressions, as for ORMs nullables usually don't apply
        // #nullable disable warnings
        // return (TSource x) => new TTarget { ... };
        // #nullable enable warnings
        var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();

        var delegateMappingSyntax = delegateMapping.Build(lambdaCtx);
        var projectionLambda = Lambda(lambdaSourceName, delegateMappingSyntax);
        var returnStatement = ctx.SyntaxFactory.Return(projectionLambda);
        return [ctx.SyntaxFactory.DisableNullableWarnings(returnStatement)];
    }
}
