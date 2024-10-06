using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public sealed record CompilationContext(
    Compilation Compilation,
    WellKnownTypes Types,
    ImmutableArray<Compilation> NestedCompilations,
    FileNameBuilder FileNameBuilder
)
{
    public SemanticModel? GetSemanticModel(SyntaxTree tree)
    {
        if (Compilation.ContainsSyntaxTree(tree))
        {
            return Compilation.GetSemanticModel(tree);
        }

        foreach (var compilation in NestedCompilations)
        {
            if (compilation.ContainsSyntaxTree(tree))
            {
                return compilation.GetSemanticModel(tree);
            }
        }

        return null;
    }
}
