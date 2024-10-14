using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols.Members;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.Capacity;

/// <summary>
/// Ensures the capacity of a collection by calling `EnsureCapacity(int)`
/// </summary>
internal class EnsureCapacityMethodSetter : IMemberSetter
{
    public static readonly EnsureCapacityMethodSetter Instance = new();

    public const string EnsureCapacityMethodName = "EnsureCapacity";

    private EnsureCapacityMethodSetter() { }

    public bool SupportsCoalesceAssignment => false;

    public ExpressionSyntax BuildAssignment(ExpressionSyntax? baseAccess, ExpressionSyntax valueToAssign, bool coalesceAssignment = false)
    {
        if (baseAccess == null)
            throw new ArgumentNullException(nameof(baseAccess));

        return InvocationWithoutIndention(MemberAccess(baseAccess, EnsureCapacityMethodName), valueToAssign);
    }
}
