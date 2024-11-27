using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    private static readonly IdentifierNameSyntax _nameofIdentifier = IdentifierName("nameof");

    private static readonly Regex _formattableStringPlaceholder = new(
        @"\{(?<placeholder>\d+)\}",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100)
    );

    public static InvocationExpressionSyntax NameOf(ExpressionSyntax expression) =>
        InvocationWithoutIndention(_nameofIdentifier, expression);

    public static IdentifierNameSyntax FullyQualifiedIdentifier(ITypeSymbol typeSymbol) =>
        IdentifierName(typeSymbol.FullyQualifiedIdentifierName());

    public static InterpolatedStringExpressionSyntax InterpolatedString(FormattableString str)
    {
        var matches = _formattableStringPlaceholder.Matches(str.Format);
        var contents = new List<InterpolatedStringContentSyntax>();
        var previousIndex = 0;
        foreach (Match match in matches)
        {
            var text = str.Format.Substring(previousIndex, match.Index - previousIndex);
            contents.Add(InterpolatedStringText(text));

            var arg = str.GetArgument(int.Parse(match.Groups["placeholder"].Value, CultureInfo.InvariantCulture));
            InterpolatedStringContentSyntax argSyntax = arg switch
            {
                ExpressionSyntax x => Interpolation(x),
                string x => InterpolatedStringText(x),
                _ => throw new InvalidOperationException(arg?.GetType() + " cannot be converted into a string interpolation"),
            };
            contents.Add(argSyntax);
            previousIndex = match.Index + match.Length;
        }

        if (previousIndex <= str.Format.Length)
        {
            contents.Add(InterpolatedStringText(str.Format.Substring(previousIndex)));
        }

        return InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken)).WithContents(List(contents));
    }

    private static InterpolatedStringTextSyntax InterpolatedStringText(string text)
    {
        return SyntaxFactory.InterpolatedStringText(
            Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken, text, text, SyntaxTriviaList.Empty)
        );
    }
}
