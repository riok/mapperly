using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Symbols.Members;

public interface IMemberSetter
{
    bool SupportsCoalesceAssignment { get; }

    ExpressionSyntax BuildAssignment(
        ExpressionSyntax? baseAccess,
        ExpressionSyntax valueToAssign,
        INamedTypeSymbol? containingType = null,
        bool coalesceAssignment = false
    );
}
