using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A member initializer which initializes null members to new objects.
/// </summary>
[DebuggerDisplay("MemberNullAssignmentInitializerMapping({_pathToInitialize} ??= new())")]
public class MemberNullAssignmentInitializerMapping : MemberAssignmentMappingContainer
{
    private readonly MemberPath _pathToInitialize;

    public MemberNullAssignmentInitializerMapping(MemberPath pathToInitialize)
    {
        _pathToInitialize = pathToInitialize;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // source.Value ??= new();
        var initializer = ExpressionStatement(
            Assignment(
                _pathToInitialize.BuildAccess(targetAccess),
                ImplicitObjectCreationExpression(),
                SyntaxKind.CoalesceAssignmentExpression
            )
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
