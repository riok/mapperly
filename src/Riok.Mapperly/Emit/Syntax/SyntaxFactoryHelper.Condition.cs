using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public IfStatementSyntax If(ExpressionSyntax condition, StatementSyntax statement, StatementSyntax? elseStatement = null)
    {
        var elseClause = elseStatement == null ? null : ElseClause(BlockIfNotReturnOrThrow(elseStatement)).AddLeadingLineFeed(Indentation);
        return IfStatement(
            LeadingLineFeedToken(SyntaxKind.IfKeyword),
            LeadingSpacedToken(SyntaxKind.OpenParenToken),
            condition,
            Token(SyntaxKind.CloseParenToken),
            BlockIfNotReturnOrThrow(statement),
            elseClause
        );
    }

    public IfStatementSyntax If(
        ExpressionSyntax condition,
        IEnumerable<StatementSyntax> statements,
        IEnumerable<StatementSyntax>? elseStatements = null
    )
    {
        ElseClauseSyntax? elseClause = null;
        if (elseStatements != null)
        {
            elseClause = ElseClause(Block(elseStatements)).AddLeadingLineFeed(Indentation);
        }

        return IfStatement(
            LeadingLineFeedTrailingSpaceToken(SyntaxKind.IfKeyword),
            Token(SyntaxKind.OpenParenToken),
            condition,
            Token(SyntaxKind.CloseParenToken),
            Block(statements),
            elseClause
        );
    }

    public static ConditionalExpressionSyntax Conditional(ExpressionSyntax condition, ExpressionSyntax whenTrue, ExpressionSyntax whenFalse)
    {
        return ConditionalExpression(
            condition,
            SpacedToken(SyntaxKind.QuestionToken),
            whenTrue,
            SpacedToken(SyntaxKind.ColonToken),
            whenFalse
        );
    }

    public static ExpressionSyntax Equal(ExpressionSyntax left, ExpressionSyntax right) =>
        BinaryExpression(SyntaxKind.EqualsExpression, left, right);

    public static ExpressionSyntax IfNoneNull(params (ITypeSymbol Type, ExpressionSyntax Access)[] values)
    {
        var conditions = values.Where(x => x.Type.IsNullable()).Select(x => IsNotNull(x.Access));
        return And(conditions);
    }

    public static ExpressionSyntax IfAnyNull(params (ITypeSymbol Type, ExpressionSyntax Access)[] values)
    {
        var conditions = values.Where(x => x.Type.IsNullable()).Select(x => IsNull(x.Access));
        return Or(conditions);
    }

    public static BinaryExpressionSyntax IsNull(ExpressionSyntax expression) =>
        BinaryExpression(SyntaxKind.EqualsExpression, expression, NullLiteral());

    public static BinaryExpressionSyntax IsNotNull(ExpressionSyntax expression) =>
        BinaryExpression(SyntaxKind.NotEqualsExpression, expression, NullLiteral());

    public static ExpressionSyntax IsNotNullWithPattern(ExpressionSyntax expression, string nullGuardedValue) =>
        IsPatternExpression(
                expression.WithTrailingTrivia(TriviaList(Space)),
                PropertyPatternClause().WithDesignation(SingleVariableDesignation(Identifier(nullGuardedValue)))
            )
            .WithIsKeyword(Token(TriviaList(), SyntaxKind.IsKeyword, TriviaList(Space)));

    public static BinaryExpressionSyntax Is(ExpressionSyntax left, ExpressionSyntax right) =>
        BinaryExpression(SyntaxKind.IsExpression, left, right);

    public static ExpressionSyntax Or(IEnumerable<ExpressionSyntax?> values) => BinaryExpression(SyntaxKind.LogicalOrExpression, values);

    public static ExpressionSyntax And(params ExpressionSyntax?[] values) => And((IEnumerable<ExpressionSyntax?>)values);

    public static ExpressionSyntax And(IEnumerable<ExpressionSyntax?> values) => BinaryExpression(SyntaxKind.LogicalAndExpression, values);

    public static ExpressionSyntax Add(ExpressionSyntax one, ExpressionSyntax two) => BinaryExpression(SyntaxKind.AddExpression, one, two);

    private static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right)
    {
        var binaryExpression = SyntaxFactory.BinaryExpression(kind, left, right);
        return binaryExpression.WithOperatorToken(SpacedToken(binaryExpression.OperatorToken.Kind()));
    }

    private static ExpressionSyntax BinaryExpression(SyntaxKind kind, params ExpressionSyntax?[] values) =>
        BinaryExpression(kind, (IEnumerable<ExpressionSyntax?>)values);

    private static ExpressionSyntax BinaryExpression(SyntaxKind kind, IEnumerable<ExpressionSyntax?> values) =>
        values.WhereNotNull().Aggregate((left, right) => BinaryExpression(kind, left, right));

    private static RecursivePatternSyntax PropertyPatternClause() =>
        RecursivePattern()
            .WithPropertyPatternClause(
                SyntaxFactory
                    .PropertyPatternClause()
                    .WithOpenBraceToken(Token(TriviaList(), SyntaxKind.OpenBraceToken, TriviaList(Space)))
                    .WithCloseBraceToken(Token(TriviaList(), SyntaxKind.CloseBraceToken, TriviaList(Space)))
            );
}
