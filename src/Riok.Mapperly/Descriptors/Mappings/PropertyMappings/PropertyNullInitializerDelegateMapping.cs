using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// A property initializer which initializes null properties to a new objects.
/// </summary>
[DebuggerDisplay("PropertyNullInitializerDelegateMapping({_pathToInitialize} ??= new())")]
public class PropertyNullInitializerDelegateMapping : IPropertyMapping
{
    private readonly PropertyPath _pathToInitialize;

    public PropertyNullInitializerDelegateMapping(PropertyPath pathToInitialize)
    {
        _pathToInitialize = pathToInitialize;
    }

    public StatementSyntax Build(ExpressionSyntax sourceAccess, ExpressionSyntax targetAccess)
    {
        // source.Value ??= new();
        return ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.CoalesceAssignmentExpression,
                _pathToInitialize.BuildAccess(targetAccess),
                ImplicitObjectCreationExpression()));
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((PropertyNullInitializerDelegateMapping)obj);
    }

    public override int GetHashCode()
        => _pathToInitialize.GetHashCode();

    public static bool operator ==(PropertyNullInitializerDelegateMapping? left, PropertyNullInitializerDelegateMapping? right)
        => Equals(left, right);

    public static bool operator !=(PropertyNullInitializerDelegateMapping? left, PropertyNullInitializerDelegateMapping? right)
        => !Equals(left, right);

    protected bool Equals(PropertyNullInitializerDelegateMapping other)
        => _pathToInitialize.Equals(other._pathToInitialize);
}
