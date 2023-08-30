using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static SimpleLambdaExpressionSyntax Lambda(string paramName, ExpressionSyntax body)
    {
        return SimpleLambdaExpression(SyntaxFactory.Parameter(Identifier(paramName)))
            .WithExpressionBody(body)
            .WithArrowToken(SpacedToken(SyntaxKind.EqualsGreaterThanToken));
    }
}
