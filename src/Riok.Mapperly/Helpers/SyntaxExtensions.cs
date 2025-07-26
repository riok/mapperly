using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Helpers;

internal static class SyntaxExtensions
{
    private const string NameOfOperatorName = "nameof";

    internal static bool TryGetFullNameOfSyntax(
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
}
