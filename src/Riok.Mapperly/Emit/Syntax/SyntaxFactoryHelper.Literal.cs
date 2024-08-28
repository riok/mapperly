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

    public static LiteralExpressionSyntax IntLiteral(int i) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));

    public static LiteralExpressionSyntax IntLiteral(uint i) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));

    public static LiteralExpressionSyntax StringLiteral(string content) =>
        LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(content));

    public static LiteralExpressionSyntax CharLiteral(char content) =>
        LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(content));

    public static LiteralExpressionSyntax LongLiteral(long content) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(content));

    public static LiteralExpressionSyntax LongLiteral(ulong content) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(content));

    public static LiteralExpressionSyntax DecimalLiteral(decimal content) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(content));

    public static LiteralExpressionSyntax DoubleLiteral(double content) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(content));

    public static LiteralExpressionSyntax FloatLiteral(float content) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(content));

    public static LiteralExpressionSyntax Literal(object? obj)
    {
        return obj switch
        {
            null => DefaultLiteral(),
            int i => IntLiteral(i),
            uint i => IntLiteral(i),
            bool b => BooleanLiteral(b),
            string s => StringLiteral(s),
            char c => CharLiteral(c),
            long l => LongLiteral(l),
            ulong l => LongLiteral(l),
            decimal d => DecimalLiteral(d),
            double d => DoubleLiteral(d),
            float f => FloatLiteral(f),
            _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, "Unsupported literal type " + obj.GetType()),
        };
    }
}
