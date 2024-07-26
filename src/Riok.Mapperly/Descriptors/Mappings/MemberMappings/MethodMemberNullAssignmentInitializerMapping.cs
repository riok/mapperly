using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A member initializer which initializes null members to new objects.
/// </summary>
[DebuggerDisplay("MemberNullAssignmentInitializerMapping({_targetPathToInitialize} ??= new())")]
public class MethodMemberNullAssignmentInitializerMapping(MemberPathSetter targetPathToInitialize, MemberPathGetter sourcePathToInitialize)
    : MemberAssignmentMappingContainer
{
    private readonly MemberPathSetter _targetPathToInitialize = targetPathToInitialize;

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // target.Value ?? new()
        var initializer = SyntaxFactoryHelper.Coalesce(
            sourcePathToInitialize.BuildAccess(targetAccess),
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

        var other = (MethodMemberNullAssignmentInitializerMapping)obj;
        return _targetPathToInitialize.Equals(other._targetPathToInitialize);
    }

    public override int GetHashCode() => _targetPathToInitialize.GetHashCode();
}
