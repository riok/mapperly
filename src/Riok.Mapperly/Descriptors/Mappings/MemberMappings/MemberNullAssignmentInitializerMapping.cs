using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A member initializer which initializes null members to new objects.
/// </summary>
[DebuggerDisplay("MemberNullAssignmentInitializerMapping({_pathToInitialize} ??= new())")]
public class MemberNullAssignmentInitializerMapping(MemberPathSetterBuilder pathToInitialize) : MemberAssignmentMappingContainer
{
    private readonly MemberPathSetterBuilder _pathToInitialize = pathToInitialize;

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // source.Value ??= new();
        var initializer = ctx.SyntaxFactory.ExpressionStatement(
            _pathToInitialize.BuildAssignment(targetAccess, ImplicitObjectCreationExpression(), true)
        );
        return base.Build(ctx, targetAccess).Prepend(initializer);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((MemberNullAssignmentInitializerMapping)obj);
    }

    public override int GetHashCode() => _pathToInitialize.GetHashCode();

    public static bool operator ==(MemberNullAssignmentInitializerMapping? left, MemberNullAssignmentInitializerMapping? right) =>
        Equals(left, right);

    public static bool operator !=(MemberNullAssignmentInitializerMapping? left, MemberNullAssignmentInitializerMapping? right) =>
        !Equals(left, right);

    protected bool Equals(MemberNullAssignmentInitializerMapping other) => _pathToInitialize.Equals(other._pathToInitialize);
}
