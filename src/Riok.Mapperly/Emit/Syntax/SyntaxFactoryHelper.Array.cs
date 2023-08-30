using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static ArrayCreationExpressionSyntax CreateArray(ArrayTypeSyntax type)
    {
        return ArrayCreationExpression(TrailingSpacedToken(SyntaxKind.NewKeyword), type, default);
    }
}
