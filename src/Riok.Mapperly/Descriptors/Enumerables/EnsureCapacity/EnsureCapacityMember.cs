using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

/// <summary>
/// Represents a call to EnsureCapacity on a collection where both the source and targets sizes are accessible.
/// </summary>
/// <remarks>
/// <code>
/// target.EnsureCapacity(source.Length + target.Count);
/// </code>
/// </remarks>
public class EnsureCapacityMember(IMemberGetter? targetAccessor, IMemberGetter sourceAccessor) : EnsureCapacityInfo
{
    public override StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return EnsureCapacityStatement(
            ctx.SyntaxFactory,
            target,
            sourceAccessor.BuildAccess(ctx.Source),
            targetAccessor?.BuildAccess(target)
        );
    }
}
