using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public ReturnStatementSyntax Return(ExpressionSyntax? expression = default)
    {
        return expression == null
            ? ReturnStatement(default, LeadingLineFeedToken(SyntaxKind.ReturnKeyword), null, Token(SyntaxKind.SemicolonToken))
            : ReturnStatement(
                default,
                LeadingLineFeedTrailingSpaceToken(SyntaxKind.ReturnKeyword),
                expression,
                Token(SyntaxKind.SemicolonToken)
            );
    }

    public StatementSyntax ReturnVariable(string identifierName) => Return(IdentifierName(identifierName));
}
