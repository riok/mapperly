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
        var conditions = values.Where(x => x.Type.IsNullable()).Select(x => IsNotNull(x.Access, x.Type));
        return And(conditions);
    }

    public static ExpressionSyntax IfAnyNull(params (ITypeSymbol Type, ExpressionSyntax Access)[] values)
    {
        var conditions = values.Where(x => x.Type.IsNullable()).Select(x => IsNull(x.Access, x.Type));
        return Or(conditions);
    }

    public static BinaryExpressionSyntax IsNull(ExpressionSyntax expression, ITypeSymbol? type = null) =>
        BinaryExpression(SyntaxKind.EqualsExpression, expression, NullLiteral(type, WellKnownMemberNames.EqualityOperatorName));

    public static BinaryExpressionSyntax IsNotNull(ExpressionSyntax expression, ITypeSymbol? type = null) =>
        BinaryExpression(SyntaxKind.NotEqualsExpression, expression, NullLiteral(type, WellKnownMemberNames.InequalityOperatorName));

    /// <summary>
    /// Builds a <c>null</c> literal. If comparing <paramref name="type"/> against <c>null</c> using the operator
    /// <paramref name="operatorMetadataName"/> would be ambiguous (multiple user defined overloads), the literal is
    /// cast to the nullable type (e.g. <c>(global::Code?)null</c>) to avoid an ambiguous operator resolution (CS9342)
    /// in the generated null check. See https://github.com/riok/mapperly/issues/2316.
    /// </summary>
    private static ExpressionSyntax NullLiteral(ITypeSymbol? type, string operatorMetadataName) =>
        type?.HasAmbiguousNullComparisonOperator(operatorMetadataName) == true
            ? CastExpression(NullableType(NonNullableIdentifier(type)), NullLiteral())
            : NullLiteral();

    public static BinaryExpressionSyntax Is(ExpressionSyntax left, ExpressionSyntax right) =>
        BinaryExpression(SyntaxKind.IsExpression, left, right);

    public static ExpressionSyntax Or(IEnumerable<ExpressionSyntax?> values) => BinaryExpression(SyntaxKind.LogicalOrExpression, values);

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
}
