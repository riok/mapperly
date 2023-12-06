using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static PatternSyntax OrPattern(IEnumerable<ExpressionSyntax?> values) =>
        values
            .WhereNotNull()
            .Select<ExpressionSyntax, PatternSyntax>(ConstantPattern)
            .Aggregate((left, right) => BinaryPattern(SyntaxKind.OrPattern, left, right));

    public static IsPatternExpressionSyntax IsPattern(ExpressionSyntax expression, PatternSyntax pattern) =>
        IsPatternExpression(expression, SpacedToken(SyntaxKind.IsKeyword), pattern);

    public static DeclarationPatternSyntax DeclarationPattern(ITypeSymbol type, string designation) =>
        SyntaxFactory.DeclarationPattern(
            FullyQualifiedIdentifier(type).AddTrailingSpace(),
            SingleVariableDesignation(Identifier(designation))
        );

    private static BinaryPatternSyntax BinaryPattern(SyntaxKind kind, PatternSyntax left, PatternSyntax right)
    {
        var binaryPattern = SyntaxFactory.BinaryPattern(kind, left, right);
        return binaryPattern.WithOperatorToken(SpacedToken(binaryPattern.OperatorToken.Kind()));
    }
}
