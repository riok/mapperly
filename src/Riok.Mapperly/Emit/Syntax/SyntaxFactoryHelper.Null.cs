using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static AssignmentExpressionSyntax CoalesceAssignment(ExpressionSyntax target, ExpressionSyntax source)
    {
        return AssignmentExpression(
            SyntaxKind.CoalesceAssignmentExpression,
            target,
            SpacedToken(SyntaxKind.QuestionQuestionEqualsToken),
            source
        );
    }

    public static SyntaxTrivia Nullable(bool enabled)
    {
        return Trivia(NullableDirectiveTrivia(LeadingSpacedToken(enabled ? SyntaxKind.EnableKeyword : SyntaxKind.DisableKeyword), true));
    }

    public static BinaryExpressionSyntax Coalesce(ExpressionSyntax expr, ExpressionSyntax coalesceExpr) =>
        BinaryExpression(SyntaxKind.CoalesceExpression, expr, coalesceExpr);

    public static IdentifierNameSyntax NonNullableIdentifier(ITypeSymbol t) => FullyQualifiedIdentifier(t.NonNullable());

    public static ExpressionSyntax NullSubstitute(ITypeSymbol t, ExpressionSyntax argument, NullFallbackValue nullFallbackValue)
    {
        return nullFallbackValue switch
        {
            NullFallbackValue.Default => DefaultExpression(FullyQualifiedIdentifier(t)),
            NullFallbackValue.EmptyString => StringLiteral(string.Empty),
            NullFallbackValue.CreateInstance => CreateInstance(t.NonNullable()),
            _ when argument is ElementAccessExpressionSyntax memberAccess
                => ThrowNullReferenceException(
                    InterpolatedString(
                        $"Sequence {NameOf(memberAccess.Expression)}, contained a null value at index {memberAccess.ArgumentList.Arguments[0].Expression}."
                    )
                ),
            _ => ThrowArgumentNullException(argument),
        };
    }

    public StatementSyntax IfNullReturnOrThrow(ExpressionSyntax expression, ExpressionSyntax? returnOrThrowExpression = null)
    {
        StatementSyntax ifExpression = returnOrThrowExpression switch
        {
            ThrowExpressionSyntax throwSyntax => ThrowStatement(throwSyntax.Expression),
            _ => AddIndentation().Return(returnOrThrowExpression),
        };

        return If(IsNull(expression), ifExpression);
    }
}
