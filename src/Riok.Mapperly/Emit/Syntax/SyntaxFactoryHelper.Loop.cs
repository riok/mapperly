using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public ForEachStatementSyntax ForEach(
        string identifierName,
        ExpressionSyntax enumerableExpression,
        params ExpressionSyntax[] expressions
    )
    {
        return ForEachStatement(
            default,
            default,
            LeadingLineFeedTrailingSpaceToken(SyntaxKind.ForEachKeyword),
            Token(SyntaxKind.OpenParenToken),
            VarIdentifier,
            Identifier(identifierName),
            SpacedToken(SyntaxKind.InKeyword),
            enumerableExpression,
            Token(SyntaxKind.CloseParenToken),
            Block(expressions)
        );
    }

    public ForStatementSyntax IncrementalForLoop(
        string counterName,
        ExpressionSyntax maxValueExclusive,
        params ExpressionSyntax[] expressions
    )
    {
        var counterDeclaration = DeclareVariable(counterName, IntLiteral(0));
        var counterIncrement = PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName(counterName));
        var condition = BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName(counterName), maxValueExclusive);
        return ForStatement(
            default,
            LeadingLineFeedTrailingSpaceToken(SyntaxKind.ForKeyword),
            Token(SyntaxKind.OpenParenToken),
            counterDeclaration,
            default,
            TrailingSpacedToken(SyntaxKind.SemicolonToken),
            condition,
            TrailingSpacedToken(SyntaxKind.SemicolonToken),
            SingletonSeparatedList<ExpressionSyntax>(counterIncrement),
            Token(SyntaxKind.CloseParenToken),
            Block(expressions)
        );
    }

    public ForStatementSyntax DecrementalForLoop(
        string counterName,
        ExpressionSyntax maxValueExclusive,
        params ExpressionSyntax[] expressions
    )
    {
        var maxValueInclusive = BinaryExpression(SyntaxKind.SubtractExpression, maxValueExclusive, IntLiteral(1));
        var counterDeclaration = DeclareVariable(counterName, maxValueInclusive);
        var counterDecrement = PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, IdentifierName(counterName));
        var condition = BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, IdentifierName(counterName), IntLiteral(0));
        return ForStatement(
            default,
            LeadingLineFeedTrailingSpaceToken(SyntaxKind.ForKeyword),
            Token(SyntaxKind.OpenParenToken),
            counterDeclaration,
            default,
            TrailingSpacedToken(SyntaxKind.SemicolonToken),
            condition,
            TrailingSpacedToken(SyntaxKind.SemicolonToken),
            SingletonSeparatedList<ExpressionSyntax>(counterDecrement),
            Token(SyntaxKind.CloseParenToken),
            Block(expressions)
        );
    }
}
