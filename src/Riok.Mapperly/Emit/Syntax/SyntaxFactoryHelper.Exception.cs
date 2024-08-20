using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    private const string NoMappingComment = "// Could not generate mapping";

    private const string ArgumentOutOfRangeExceptionClassName = "System.ArgumentOutOfRangeException";
    private const string ArgumentNullExceptionClassName = "System.ArgumentNullException";
    private const string ArgumentExceptionClassName = "System.ArgumentException";
    private const string NotImplementedExceptionClassName = "System.NotImplementedException";
    private const string NullReferenceExceptionClassName = "System.NullReferenceException";

    public static ThrowExpressionSyntax ThrowNullReferenceException(string message) => ThrowNullReferenceException(StringLiteral(message));

    private static ThrowExpressionSyntax ThrowNullReferenceException(ExpressionSyntax arg) =>
        Throw(NullReferenceExceptionClassName, ArgumentListWithoutIndention([arg]));

    public static ThrowExpressionSyntax ThrowArgumentOutOfRangeException(ExpressionSyntax arg, string message) =>
        Throw(ArgumentOutOfRangeExceptionClassName, ArgumentListWithoutIndention([NameOf(arg), arg, StringLiteral(message)]));

    public static ThrowExpressionSyntax ThrowArgumentNullException(ExpressionSyntax arg) =>
        Throw(ArgumentNullExceptionClassName, ArgumentListWithoutIndention([NameOf(arg)]));

    public static ThrowExpressionSyntax ThrowArgumentExpression(ExpressionSyntax message, ExpressionSyntax arg) =>
        Throw(ArgumentExceptionClassName, ArgumentListWithoutIndention([message, NameOf(arg)]));

    public ThrowExpressionSyntax ThrowMappingNotImplementedException()
    {
        return Throw(NotImplementedExceptionClassName, SyntaxFactory.ArgumentList()).AddLeadingLineComment(NoMappingComment, Indentation);
    }

    private ThrowStatementSyntax ThrowStatement(ExpressionSyntax? expression = null)
    {
        return SyntaxFactory.ThrowStatement(
            default,
            LeadingLineFeedTrailingSpaceToken(SyntaxKind.ThrowKeyword),
            expression,
            Token(SyntaxKind.SemicolonToken)
        );
    }

    private static ThrowExpressionSyntax Throw(string name, ArgumentListSyntax args)
    {
        var ex = CreateObject(IdentifierName(name), args);
        return ThrowExpression(TrailingSpacedToken(SyntaxKind.ThrowKeyword), ex);
    }
}
