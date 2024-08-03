using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping where the target type accepts IEnumerable as a single argument.
/// </summary>
public class LinqConstructorMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    INewInstanceMapping elementMapping,
    string? selectMethod
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

        return ctx.SyntaxFactory.CreateInstance(TargetType, mappedSource);
    }
}
