using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

/// <summary>
/// Represents a call to EnsureCapacity on a collection where both the source and targets sizes are accessible.
/// </summary>
/// <remarks>
/// <code>
/// target.EnsureCapacity(source.Length + target.Count);
/// </code>
/// </remarks>
public class EnsureCapacityMember(string targetAccessor, string sourceAccessor) : EnsureCapacityInfo
{
    public override StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return EnsureCapacityStatement(
            ctx.SyntaxFactory,
            target,
            MemberAccess(ctx.Source, sourceAccessor),
            MemberAccess(target, targetAccessor)
        );
    }
}
