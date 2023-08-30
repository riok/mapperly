using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public SwitchExpressionSyntax Switch(ExpressionSyntax governingExpression, IEnumerable<SwitchExpressionArmSyntax> arms)
    {
        return SwitchExpression(
            governingExpression,
            LeadingSpacedToken(SyntaxKind.SwitchKeyword),
            Token(SyntaxKind.OpenBraceToken).AddLeadingLineFeed(Indentation).AddTrailingLineFeed(Indentation + 1),
            AddIndentation().CommaLineFeedSeparatedList(arms),
            LeadingLineFeedToken(SyntaxKind.CloseBraceToken)
        );
    }

    public static SwitchExpressionArmSyntax SwitchArm(PatternSyntax pattern, ExpressionSyntax expression)
    {
        return SwitchExpressionArm(pattern, default, SpacedToken(SyntaxKind.EqualsGreaterThanToken), expression);
    }

    public static WhenClauseSyntax SwitchWhen(ExpressionSyntax condition) => WhenClause(SpacedToken(SyntaxKind.WhenKeyword), condition);
}
