using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Output;

public readonly record struct MapperNode(string FileName, CompilationUnitSyntax Body)
{
    public bool Equals(MapperNode other)
    {
        return string.Equals(FileName, other.FileName, StringComparison.Ordinal) && Body.IsEquivalentTo(other.Body);
    }

    public override int GetHashCode() => HashCode.Combine(FileName, Body.SyntaxTree.Length);
}
