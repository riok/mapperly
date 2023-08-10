using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a simple member mapping including an assignment to a target member.
/// (eg. target.A = source.B)
/// </summary>
[DebuggerDisplay("MemberAssignmentMapping({SourcePath.FullName} => {TargetPath.FullName})")]
public class MemberAssignmentMapping : IMemberAssignmentMapping
{
    private readonly IMemberMapping _mapping;
    private readonly SetterMemberPath _targetPath;

    public MemberAssignmentMapping(SetterMemberPath targetPath, IMemberMapping mapping)
    {
        _targetPath = targetPath;
        _mapping = mapping;
    }

    public GetterMemberPath SourcePath => _mapping.SourcePath;

    public MemberPath TargetPath => _targetPath;

    public ITypeSymbol SourceType => _mapping.SourceType;

    public ITypeSymbol TargetType => _mapping.TargetType;

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess) =>
        ctx.SyntaxFactory.SingleStatement(BuildExpression(ctx, targetAccess));

    public ExpressionSyntax BuildExpression(TypeMappingBuildContext ctx, ExpressionSyntax? targetAccess)
    {
        var mappedValue = _mapping.Build(ctx);

        // target.SetValue(source.Value); or target.Value = source.Value;
        return _targetPath.BuildAssignment(targetAccess, mappedValue);
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

    public static bool operator ==(MemberAssignmentMapping? left, MemberAssignmentMapping? right) => Equals(left, right);

    public static bool operator !=(MemberAssignmentMapping? left, MemberAssignmentMapping? right) => !Equals(left, right);

    protected bool Equals(MemberAssignmentMapping other)
    {
        return _mapping.Equals(other._mapping) && SourcePath.Equals(other.SourcePath) && TargetPath.Equals(other.TargetPath);
    }
}
