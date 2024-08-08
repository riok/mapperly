using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A member initializer which initializes null members to new objects.
/// </summary>
[DebuggerDisplay("MemberNullAssignmentInitializerMapping({_targetPathToInitialize} ??= new())")]
public class MethodMemberNullAssignmentInitializerMapping(
    MemberPathSetter targetPathToInitialize,
    MemberPathGetter sourcePathToInitialize,
    IInstanceConstructor constructor
) : MemberAssignmentMappingContainer
{
    private readonly MemberPathSetter _targetPathToInitialize = targetPathToInitialize;

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // new T();
        var newTarget = constructor.CreateInstance(ctx);

        // target.Value = new T();
        var setTarget = _targetPathToInitialize.BuildAssignment(targetAccess, newTarget);

        // if (target.Value == null) target.Value = new T();
        var setTargetIfNull = ctx.SyntaxFactory.IfNull(sourcePathToInitialize.BuildAccess(targetAccess), setTarget);

        return base.Build(ctx, targetAccess).Prepend(setTargetIfNull);
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
