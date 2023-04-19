using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

/// <summary>
/// Represents a call to EnsureCapacity on a collection where both the source and targets sizes are accessible.
/// </summary>
/// <remarks>
/// <code>
/// target.EnsureCapacity(source.Length + target.Count);
/// </code>
/// </remarks>
public class EnsureCapacityMember : EnsureCapacity
{
    private readonly string _targetAccessor;
    private readonly string _sourceAccessor;

    public EnsureCapacityMember(string targetAccessor, string sourceAccessor)
    {
        _targetAccessor = targetAccessor;
        _sourceAccessor = sourceAccessor;
    }

    public override StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return EnsureCapacityStatement(target, MemberAccess(ctx.Source, _sourceAccessor), MemberAccess(target, _targetAccessor));
    }
}
