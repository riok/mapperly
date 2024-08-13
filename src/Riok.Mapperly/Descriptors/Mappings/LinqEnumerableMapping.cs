using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping which works by using linq (select + collect).
/// </summary>
public class LinqEnumerableMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    INewInstanceMapping elementMapping,
    string? selectMethod,
    string? collectMethod
) : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ExpressionSyntax mappedSource;

        // Select / Map if needed
        if (selectMethod != null)
        {
            var (lambdaCtx, lambdaSourceName) = ctx.WithNewScopedSource();
            var sourceMapExpression = elementMapping.Build(lambdaCtx);
            var convertLambda = Lambda(lambdaSourceName, sourceMapExpression);
            mappedSource = ctx.SyntaxFactory.Invocation(selectMethod, ctx.Source, convertLambda);
        }
        else
        {
            mappedSource = elementMapping.Build(ctx);
        }

        return collectMethod == null ? mappedSource : ctx.SyntaxFactory.Invocation(collectMethod, mappedSource);
    }
}
