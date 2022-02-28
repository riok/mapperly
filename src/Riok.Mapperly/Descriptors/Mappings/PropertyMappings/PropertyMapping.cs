using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a simple property mapping (eg. target.A = source.B)
/// </summary>
[DebuggerDisplay("PropertyMapping({SourcePath.FullName} => {TargetPath.FullName})")]
public class PropertyMapping : IPropertyMapping
{
    private readonly TypeMapping _mapping;
    private readonly bool _nullConditionalSourceAccess;

    public PropertyMapping(
        PropertyPath sourcePath,
        PropertyPath targetPath,
        TypeMapping mapping,
        bool nullConditionalSourceAccess)
    {
        SourcePath = sourcePath;
        TargetPath = targetPath;
        _mapping = mapping;
        _nullConditionalSourceAccess = nullConditionalSourceAccess;
    }

    public PropertyPath SourcePath { get; }

    public PropertyPath TargetPath { get; }

    public StatementSyntax Build(
        ExpressionSyntax sourceAccess,
        ExpressionSyntax targetAccess)
    {
        var sourcePropertyAccess = SourcePath.BuildAccess(sourceAccess, true, _nullConditionalSourceAccess);
        var targetPropertyAccess = TargetPath.BuildAccess(targetAccess);
        var mappedValue = _mapping.Build(sourcePropertyAccess);

        // target.Property = mappedValue;
        var assignment = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            targetPropertyAccess,
            mappedValue);
        return ExpressionStatement(assignment);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((PropertyMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _mapping.GetHashCode();
            hashCode = (hashCode * 397) ^ SourcePath.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetPath.GetHashCode();
            hashCode = (hashCode * 397) ^ _nullConditionalSourceAccess.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(PropertyMapping? left, PropertyMapping? right)
        => Equals(left, right);

    public static bool operator !=(PropertyMapping? left, PropertyMapping? right)
        => !Equals(left, right);

    protected bool Equals(PropertyMapping other)
    {
        return _mapping.Equals(other._mapping)
            && SourcePath.Equals(other.SourcePath)
            && TargetPath.Equals(other.TargetPath)
            && _nullConditionalSourceAccess == other._nullConditionalSourceAccess;
    }
}
