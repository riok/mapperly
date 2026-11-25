using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public enum NullableTarget
    {
        All,
        Annotations,
        Warnings,
    }

    public static AssignmentExpressionSyntax CoalesceAssignment(ExpressionSyntax target, ExpressionSyntax source)
    {
        return AssignmentExpression(
            SyntaxKind.CoalesceAssignmentExpression,
            target,
            SpacedToken(SyntaxKind.QuestionQuestionEqualsToken),
            source
        );
    }

    public static SyntaxTrivia Nullable(bool enabled, NullableTarget target = NullableTarget.All)
    {
        var nullableDirective = NullableDirectiveTrivia(
            LeadingSpacedToken(enabled ? SyntaxKind.EnableKeyword : SyntaxKind.DisableKeyword),
            true
        );

        nullableDirective = target switch
        {
            NullableTarget.Annotations => nullableDirective.WithTargetToken(LeadingSpacedToken(SyntaxKind.AnnotationsKeyword)),
            NullableTarget.Warnings => nullableDirective.WithTargetToken(LeadingSpacedToken(SyntaxKind.WarningsKeyword)),
            _ => nullableDirective,
        };

        return Trivia(nullableDirective);
    }

    public StatementSyntax DisableNullableWarnings(StatementSyntax statement)
    {
        if (!supportedFeatures.NullableAttributes)
            return statement;

        var leadingTrivia = statement
            .GetLeadingTrivia()
            .Insert(0, ElasticCarriageReturnLineFeed)
            .Insert(1, Nullable(false, NullableTarget.Warnings));
        var trailingTrivia = statement
            .GetTrailingTrivia()
            .Insert(0, ElasticCarriageReturnLineFeed)
            .Insert(1, Nullable(true, NullableTarget.Warnings));
        return statement.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
    }

    public static BinaryExpressionSyntax Coalesce(ExpressionSyntax expr, ExpressionSyntax coalesceExpr) =>
        BinaryExpression(SyntaxKind.CoalesceExpression, expr, coalesceExpr);

    public static IdentifierNameSyntax NonNullableIdentifier(ITypeSymbol t) => FullyQualifiedIdentifier(t.NonNullable());

    public static ExpressionSyntax NullSubstitute(ITypeSymbol t, ExpressionSyntax argument, NullFallbackValue nullFallbackValue)
    {
        return nullFallbackValue switch
        {
            NullFallbackValue.Default when t.IsNullableValueType() => DefaultExpression(FullyQualifiedIdentifier(t)),
            NullFallbackValue.Default => DefaultLiteral(),
            NullFallbackValue.EmptyString => StringLiteral(string.Empty),
            NullFallbackValue.CreateInstance => CreateInstance(t),
            _ when argument is ElementAccessExpressionSyntax memberAccess => ThrowNullReferenceException(
                InterpolatedString(
                    $"Sequence {NameOf(memberAccess.Expression)}, contained a null value at index {memberAccess.ArgumentList.Arguments[0].Expression}."
                )
            ),
            _ when argument is MemberAccessExpressionSyntax or SimpleNameSyntax => ThrowArgumentNullException(argument),
            _ when argument is InvocationExpressionSyntax invocation => ThrowNullReferenceException(
                StringLiteral(invocation.Expression + " returned null")
            ),
            _ => ThrowNullReferenceException(StringLiteral(argument + " is null")),
        };
    }

    public IfStatementSyntax IfNull(ExpressionSyntax value, ExpressionSyntax body, ITypeSymbol? type = null) =>
        IfNull(value, AddIndentation().ExpressionStatement(body), type);

    public IfStatementSyntax IfNull(ExpressionSyntax value, StatementSyntax statement, ITypeSymbol? type = null) =>
        If(IsNull(value, type), statement);

    public StatementSyntax IfNullReturnOrThrow(
        ExpressionSyntax expression,
        ExpressionSyntax? returnOrThrowExpression = null,
        ITypeSymbol? type = null
    )
    {
        StatementSyntax ifExpression = returnOrThrowExpression switch
        {
            ThrowExpressionSyntax throwSyntax => AddIndentation().ThrowStatement(throwSyntax.Expression),
            _ => AddIndentation().Return(returnOrThrowExpression),
        };

        return IfNull(expression, ifExpression, type);
    }

    public static ExpressionSyntax SuppressNullableWarning(ExpressionSyntax expression) =>
        PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, expression);
}
