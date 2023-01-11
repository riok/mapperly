using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a simple property mapping including an assignment to a target property.
/// (eg. target.A = source.B)
/// </summary>
[DebuggerDisplay("PropertyMapping({SourcePath.FullName} => {TargetPath.FullName})")]
public class PropertyAssignmentMapping : IPropertyAssignmentMapping
{
    private readonly IPropertyMapping _mapping;

    public PropertyAssignmentMapping(
        PropertyPath targetPath,
        IPropertyMapping mapping)
    {
        TargetPath = targetPath;
        _mapping = mapping;
    }

    public PropertyPath SourcePath => _mapping.SourcePath;

    public PropertyPath TargetPath { get; }

    public StatementSyntax Build(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetAccess)
    {
        return ExpressionStatement(BuildExpression(ctx, targetAccess));
    }

    public ExpressionSyntax BuildExpression(
        TypeMappingBuildContext ctx,
        ExpressionSyntax? targetAccess)
    {
        var targetPropertyAccess = TargetPath.BuildAccess(targetAccess);
        var mappedValue = _mapping.Build(ctx);

        // target.Property = mappedValue;
        return AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            targetPropertyAccess,
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

        return Equals((PropertyAssignmentMapping)obj);
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

    public static bool operator ==(PropertyAssignmentMapping? left, PropertyAssignmentMapping? right)
        => Equals(left, right);

    public static bool operator !=(PropertyAssignmentMapping? left, PropertyAssignmentMapping? right)
        => !Equals(left, right);

    protected bool Equals(PropertyAssignmentMapping other)
    {
        return _mapping.Equals(other._mapping)
            && SourcePath.Equals(other.SourcePath)
            && TargetPath.Equals(other.TargetPath);
    }
}
