using Microsoft.CodeAnalysis.CSharp.Syntax;
#pragma warning disable CS0659

namespace Riok.Mapperly;

public readonly struct MapperNode : IEquatable<MapperNode>
{
    public MapperNode(CompilationUnitSyntax body, string fileName)
    {
        Body = body;
        FileName = fileName;
    }

    public CompilationUnitSyntax Body { get; }
    public string FileName { get; }

    public bool Equals(MapperNode other) =>
        Body.IsEquivalentTo(other.Body) && string.Equals(FileName, other.FileName, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is MapperNode other && Equals(other);
}
