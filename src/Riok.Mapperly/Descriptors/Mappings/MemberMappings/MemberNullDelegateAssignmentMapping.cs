using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// a member mapping container, which performs a null check before the mappings.
/// </summary>
[DebuggerDisplay("MemberNullDelegateAssignmentMapping({_nullConditionalSourcePath} != null)")]
public class MemberNullDelegateAssignmentMapping : MemberAssignmentMappingContainer
{
    private readonly GetterMemberPath _nullConditionalSourcePath;
    private readonly bool _throwInsteadOfConditionalNullMapping;
    private readonly bool _needsNullSafeAccess;

    public MemberNullDelegateAssignmentMapping(
        GetterMemberPath nullConditionalSourcePath,
        IMemberAssignmentMappingContainer parent,
        bool throwInsteadOfConditionalNullMapping,
        bool needsNullSafeAccess
    )
        : base(parent)
    {
        _needsNullSafeAccess = needsNullSafeAccess;
        _nullConditionalSourcePath = nullConditionalSourcePath;
        _throwInsteadOfConditionalNullMapping = throwInsteadOfConditionalNullMapping;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // if (source.Value != null)
        //   target.Value = Map(Source.Name);
        // else
        //   throw ...
        var sourceNullConditionalAccess = _nullConditionalSourcePath.BuildAccess(ctx.Source, false, _needsNullSafeAccess, true);
        var nameofSourceAccess = _nullConditionalSourcePath.BuildAccess(ctx.Source, false, false, true);
        var condition = IsNotNull(sourceNullConditionalAccess);
        var conditionCtx = ctx.AddIndentation();
        var trueClause = base.Build(conditionCtx, targetAccess);
        var elseClause = _throwInsteadOfConditionalNullMapping
            ? new[] { conditionCtx.SyntaxFactory.ExpressionStatement(ThrowArgumentNullException(nameofSourceAccess)) }
            : null;
        var ifExpression = ctx.SyntaxFactory.If(condition, trueClause, elseClause);
        return new[] { ifExpression };
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((MemberNullDelegateAssignmentMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (_nullConditionalSourcePath.GetHashCode() * 397) ^ _throwInsteadOfConditionalNullMapping.GetHashCode();
        }
    }

    public static bool operator ==(MemberNullDelegateAssignmentMapping? left, MemberNullDelegateAssignmentMapping? right) =>
        Equals(left, right);

    public static bool operator !=(MemberNullDelegateAssignmentMapping? left, MemberNullDelegateAssignmentMapping? right) =>
        !Equals(left, right);

    protected bool Equals(MemberNullDelegateAssignmentMapping other)
    {
        return _nullConditionalSourcePath.Equals(other._nullConditionalSourcePath)
            && _throwInsteadOfConditionalNullMapping == other._throwInsteadOfConditionalNullMapping;
    }
}
