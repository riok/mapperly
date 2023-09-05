using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A member initializer which initializes null members to new objects.
/// </summary>
[DebuggerDisplay("MemberNullAssignmentInitializerMapping({_targetPathToInitialize} ??= new())")]
public class MethodMemberNullAssignmentInitializerMapping : MemberAssignmentMappingContainer
{
    private readonly SetterMemberPath _targetPathToInitialize;
    private readonly GetterMemberPath _sourcePathToInitialize;

    public MethodMemberNullAssignmentInitializerMapping(SetterMemberPath targetPathToInitialize, GetterMemberPath sourcePathToInitialize)
    {
        _targetPathToInitialize = targetPathToInitialize;
        _sourcePathToInitialize = sourcePathToInitialize;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // target.Value ?? new()
        var initializer = SyntaxFactoryHelper.Coalesce(
            _sourcePathToInitialize.BuildAccess(targetAccess),
            SyntaxFactory.ImplicitObjectCreationExpression()
        );

        // target.SetValue(source.Value ?? new());
        var setTarget = ctx.SyntaxFactory.ExpressionStatement(_targetPathToInitialize.BuildAssignment(targetAccess, initializer));
        return base.Build(ctx, targetAccess).Prepend(setTarget);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((MethodMemberNullAssignmentInitializerMapping)obj);
    }

    public override int GetHashCode() => _targetPathToInitialize.GetHashCode();

    public static bool operator ==(
        MethodMemberNullAssignmentInitializerMapping? left,
        MethodMemberNullAssignmentInitializerMapping? right
    ) => Equals(left, right);

    public static bool operator !=(
        MethodMemberNullAssignmentInitializerMapping? left,
        MethodMemberNullAssignmentInitializerMapping? right
    ) => !Equals(left, right);

    protected bool Equals(MethodMemberNullAssignmentInitializerMapping other) =>
        _targetPathToInitialize.Equals(other._targetPathToInitialize);
}
