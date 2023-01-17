using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a property mapping to a reference property
/// when simple assignment cannot be used.
/// ref used for compatibility with readonly value types
/// (eg. map(source.B, ref target.A))
/// </summary>
[DebuggerDisplay("PropertyMapping({SourcePath.FullName} => {TargetPath.FullName})")]
public class PropertyReferenceMapping : IPropertyAssignmentMapping
{
    private readonly IPropertyMapping _mapping;

    public PropertyReferenceMapping(
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
        ctx = ctx.WithRefHandler(targetPropertyAccess);
        var mappingMethod = _mapping.Build(ctx);

        // MapTo(source.property, in target.Property);
        return mappingMethod;
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

    public static bool operator ==(PropertyReferenceMapping? left, PropertyReferenceMapping? right)
        => Equals(left, right);

    public static bool operator !=(PropertyReferenceMapping? left, PropertyReferenceMapping? right)
        => !Equals(left, right);

    protected bool Equals(PropertyReferenceMapping other)
    {
        return _mapping.Equals(other._mapping)
            && SourcePath.Equals(other.SourcePath)
            && TargetPath.Equals(other.TargetPath);
    }
}
