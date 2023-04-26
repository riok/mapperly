using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Tests;

public class GeneratedMethod
{
    private const string MethodIndention = "    ";

    public GeneratedMethod(MethodDeclarationSyntax declarationSyntax)
    {
        Name = declarationSyntax.Identifier.ToString();
        Signature = $"{declarationSyntax.ReturnType.ToString()} {Name}{declarationSyntax.ParameterList.ToString().Trim()}";
        Body = ExtractBody(declarationSyntax);
    }

    public string Name { get; }

    public string Signature { get; }

    public string Body { get; }

    /// <summary>
    /// Builds the method body without the method body braces and without the method body level indention.
    /// </summary>
    /// <param name="declarationSyntax">The syntax of the method.</param>
    /// <returns>The cleaned body.</returns>
    private static string ExtractBody(MethodDeclarationSyntax declarationSyntax)
    {
        var body =
            declarationSyntax.Body
                ?.NormalizeWhitespace()
                .ToFullString()
                .Trim(' ', '\r', '\n')
                .Trim('{', '}')
                .Trim(' ', '\r', '\n')
                .ReplaceLineEndings() ?? string.Empty;
        var lines = body.Split(Environment.NewLine);
        return lines.Length == 0
            ? string.Empty
            : string.Join(Environment.NewLine, lines.Select(l => l.StartsWith(MethodIndention) ? l[MethodIndention.Length..] : l));
    }
}
