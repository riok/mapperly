using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Tests;

public class GeneratedMethod
{
    public GeneratedMethod(MethodDeclarationSyntax declarationSyntax)
    {
        Name = declarationSyntax.Identifier.ToString();
        Signature = $"{declarationSyntax.ReturnType.ToString()} {Name}{declarationSyntax.ParameterList.ToString().Trim()}";
        ConstraintClauses = ExtractParameterConstraints(declarationSyntax.ConstraintClauses);
        Body = ExtractBody(declarationSyntax);
    }

    public string Name { get; }

    public string Signature { get; }

    public string? ConstraintClauses { get; }

    public string Body { get; }

    private static string? ExtractParameterConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> typeParameterConstraints)
    {
        if (typeParameterConstraints.Count == 0)
            return null;

        return typeParameterConstraints.ToFullString().Trim(' ', '\r', '\n').Trim('\r', '\n').ReplaceLineEndings();
    }

    /// <summary>
    /// Builds the method body without the method body braces and without the method body level indentation.
    /// </summary>
    /// <param name="declarationSyntax">The syntax of the method.</param>
    /// <returns>The cleaned body.</returns>
    private static string ExtractBody(MethodDeclarationSyntax declarationSyntax)
    {
        if (declarationSyntax.Body == null)
            return string.Empty;

        var body = declarationSyntax
            .Body.ToFullString()
            .Trim(' ', '\r', '\n')
            .TrimStart('{')
            .TrimEnd('}')
            .Trim('\r', '\n')
            .ReplaceLineEndings();
        var lines = body.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var indentionCount = lines[0].TakeWhile(x => x == ' ').Count();
        var indention = lines[0][..indentionCount];
        return string.Join(Environment.NewLine, lines.Select(l => l.StartsWith(indention) ? l[indentionCount..] : l)).Trim(' ', '\r', '\n');
    }
}
