#if ROSLYN3_9_OR_GREATER && !ROSLYN4_0_OR_GREATER

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly;

internal class SyntaxProvider : ISyntaxContextReceiver
{
    public IEnumerable<ClassDeclarationSyntax> ClassDeclarations => _classDeclarations;
    private readonly List<ClassDeclarationSyntax> _classDeclarations = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (!IsSyntaxTargetForGeneration(context.Node))
            return;

        var syntax = GetSemanticTargetForGeneration(context);
        if (syntax == null)
            return;

        _classDeclarations.Add(syntax);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
    {
        var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
        foreach (var attributeListSyntax in classDeclaration.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();
                if (string.Equals(fullName, MapperGenerator.MapperAttributeName, StringComparison.Ordinal))
                    return classDeclaration;
            }
        }

        return null;
    }
}
#endif
