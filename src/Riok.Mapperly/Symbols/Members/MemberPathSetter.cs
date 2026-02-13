using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Symbols.Members;

/// <summary>
/// A setter for a <see cref="MemberPath"/>.
/// </summary>
[DebuggerDisplay("{_memberPath}")]
public class MemberPathSetter
{
    private readonly NonEmptyMemberPath _memberPath;
    private readonly MemberPathGetter _baseAccessGetter;
    private readonly IMemberSetter _memberSetter;
    private readonly IMappableMember _member;

    private MemberPathSetter(
        NonEmptyMemberPath memberPath,
        MemberPathGetter baseAccessGetter,
        IMemberSetter memberSetter,
        IMappableMember member
    )
    {
        _memberPath = memberPath;
        _baseAccessGetter = baseAccessGetter;
        _memberSetter = memberSetter;
        _member = member;
    }

    public bool SupportsCoalesceAssignment => _memberSetter.SupportsCoalesceAssignment;

    public static MemberPathSetter Build(SimpleMappingBuilderContext ctx, NonEmptyMemberPath path)
    {
        Debug.Assert(path.Member.CanSet);

        var objectPath = MemberPath.Create(path.RootType, path.ObjectPath.ToList());
        var objectGetter = objectPath.BuildGetter(ctx);
        var memberSetter = path.Member.BuildSetter(ctx.UnsafeAccessorContext);
        return new MemberPathSetter(path, objectGetter, memberSetter, path.Member);
    }

    public ExpressionSyntax BuildAssignment(ExpressionSyntax? baseAccess, ExpressionSyntax valueToAssign, bool coalesceAssignment = false)
    {
        baseAccess = _baseAccessGetter.BuildAccess(baseAccess);
        return _memberSetter.BuildAssignment(baseAccess, valueToAssign, _member.ContainingType, coalesceAssignment);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (MemberPathSetter)obj;
        return _memberPath.Equals(other._memberPath);
    }

    public override int GetHashCode() => _memberPath.GetHashCode();
}
