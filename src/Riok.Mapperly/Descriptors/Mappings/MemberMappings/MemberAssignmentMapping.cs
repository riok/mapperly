using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a simple member mapping including an assignment to a target member.
/// (eg. target.A = source.B)
/// </summary>
[DebuggerDisplay("MemberAssignmentMapping({SourcePath.FullName} => {TargetPath.FullName})")]
public class MemberAssignmentMapping : IMemberAssignmentMapping
{
    private readonly IMemberMapping _mapping;

    public MemberAssignmentMapping(
        MemberPath targetPath,
        IMemberMapping mapping)
    {
        TargetPath = targetPath;
        _mapping = mapping;
    }

    public MemberPath SourcePath => _mapping.SourcePath;

    public MemberPath TargetPath { get; }

    public IEnumerable<StatementSyntax> Build(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetAccess)
    {
        return new[]
        {
            ExpressionStatement(BuildExpression(ctx, targetAccess)),
        };
    }

    public ExpressionSyntax BuildExpression(
        TypeMappingBuildContext ctx,
        ExpressionSyntax? targetAccess)
    {
        var targetMemberAccess = TargetPath.BuildAccess(targetAccess);
        var mappedValue = _mapping.Build(ctx);

        // target.Member = mappedValue;
        return Assignment(
            targetMemberAccess,
            mappedValue);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((MemberAssignmentMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _mapping.GetHashCode();
            hashCode = (hashCode * 397) ^ SourcePath.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetPath.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(MemberAssignmentMapping? left, MemberAssignmentMapping? right)
        => Equals(left, right);

    public static bool operator !=(MemberAssignmentMapping? left, MemberAssignmentMapping? right)
        => !Equals(left, right);

    protected bool Equals(MemberAssignmentMapping other)
    {
        return _mapping.Equals(other._mapping)
            && SourcePath.Equals(other.SourcePath)
            && TargetPath.Equals(other.TargetPath);
    }
}
