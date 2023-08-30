using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static LiteralExpressionSyntax DefaultLiteral() => LiteralExpression(SyntaxKind.DefaultLiteralExpression);

    public static LiteralExpressionSyntax NullLiteral() => LiteralExpression(SyntaxKind.NullLiteralExpression);

    public static LiteralExpressionSyntax BooleanLiteral(bool b) =>
        LiteralExpression(b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

    public static LiteralExpressionSyntax IntLiteral(int i) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i));

    private static LiteralExpressionSyntax StringLiteral(string content) =>
        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(content));
}
