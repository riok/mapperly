using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Tests;

public class GeneratedMethod
{

    public GeneratedMethod(MethodDeclarationSyntax declarationSyntax)
    {
        Name = declarationSyntax.Identifier.ToString();
        Signature = $"{declarationSyntax.ReturnType.ToString()} {Name}{declarationSyntax.ParameterList.ToString().Trim()}";
        Body = ExtractBody(declarationSyntax);
    }

    public string Name { get; }

    public string Signature { get; }

    public string Body { get; }

    private static string ExtractBody(MethodDeclarationSyntax declarationSyntax)
    {
        return declarationSyntax
            .Body
            ?.NormalizeWhitespace()
            .ToFullString()
            // simplify string to make assertions simpler, trim only first braces
            // but all whitespace/nl afterwards
            .Trim(' ', '\r', '\n')
            .Trim('{', '}')
            .Trim(' ', '\r', '\n')
            .ReplaceLineEndings() ?? string.Empty;
    }
}
