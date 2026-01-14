using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Output;

public readonly record struct MapperNode(string FileName, CompilationUnitSyntax Body, string? EndOfLine = null, string? Charset = null)
{
    public bool Equals(MapperNode other)
    {
        return string.Equals(FileName, other.FileName, StringComparison.Ordinal)
            && Body.IsEquivalentTo(other.Body)
            && string.Equals(EndOfLine, other.EndOfLine, StringComparison.Ordinal)
            && string.Equals(Charset, other.Charset, StringComparison.Ordinal);
    }

    public override int GetHashCode() => HashCode.Combine(FileName, Body.SyntaxTree.Length, EndOfLine, Charset);
}
