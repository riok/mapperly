using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Helpers;

internal static class SyntaxExtensions
{
    private const string NameOfOperatorName = "nameof";

    internal static bool TryGetNameOfSyntax(
        this AttributeArgumentSyntax? syntax,
        [NotNullWhen(true)] out InvocationExpressionSyntax? invocationExpression
    )
    {
        if (
            syntax?.Expression is InvocationExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.Text: NameOfOperatorName }
            } invocationExpressionSyntax
        )
        {
            invocationExpression = invocationExpressionSyntax;
            return true;
        }

        invocationExpression = null;
        return false;
    }

    public static bool IsFullNameOfSyntax(this InvocationExpressionSyntax syntax)
    {
        var argument = syntax.ArgumentList.Arguments[0];
        var firstToken = argument.GetFirstToken();
        return firstToken.IsVerbatimIdentifier();
    }
}
