using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a direct assignment mapping.
/// Source and target types need to be the same types.
/// </summary>
public class DirectAssignmentMapping : TypeMapping
{
    public DirectAssignmentMapping(ITypeSymbol type) : base(type, type)
    {
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
        => source;

    public override bool IsSynthetic => true;
}
