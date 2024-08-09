using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols.Members;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// a member mapping container, which performs a null check before the mappings.
/// </summary>
[DebuggerDisplay("MemberNullDelegateAssignmentMapping({_nullConditionalSourcePath} != null)")]
public class MemberNullDelegateAssignmentMapping(
    MemberPathGetter nullConditionalSourcePath,
    IMemberAssignmentMappingContainer parent,
    bool needsNullSafeAccess
) : MemberAssignmentMappingContainer(parent)
{
    private readonly MemberPathGetter _nullConditionalSourcePath = nullConditionalSourcePath;
    private readonly List<MemberPathSetter> _targetsToSetNull = new();
    private bool _throwOnSourcePathNull;

    public void ThrowOnSourcePathNull()
    {
        _throwOnSourcePathNull = true;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // if (source.Value != null)
        //   target.Value = Map(Source.Name);
        // else
        //   throw ...
        var sourceNullConditionalAccess = _nullConditionalSourcePath.BuildAccess(
            ctx.Source,
            addValuePropertyOnNullable: false,
            nullConditional: needsNullSafeAccess,
            skipTrailingNonNullable: true
        );
        var condition = IsNotNull(sourceNullConditionalAccess);
        var conditionCtx = ctx.AddIndentation();
        var trueClause = base.Build(conditionCtx, targetAccess);
        var elseClause = BuildElseClause(conditionCtx, targetAccess);
        var ifExpression = ctx.SyntaxFactory.If(condition, trueClause, elseClause);
        return new[] { ifExpression };
    }

    public void AddNullMemberAssignment(MemberPathSetter targetPath) => _targetsToSetNull.Add(targetPath);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (MemberNullDelegateAssignmentMapping)obj;
        return _nullConditionalSourcePath.Equals(other._nullConditionalSourcePath);
    }

    public override int GetHashCode() => _nullConditionalSourcePath.GetHashCode();

    private IEnumerable<StatementSyntax>? BuildElseClause(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        if (_throwOnSourcePathNull)
        {
            // throw new ArgumentNullException
            var nameofSourceAccess = _nullConditionalSourcePath.BuildAccess(
                ctx.Source,
                addValuePropertyOnNullable: false,
                nullConditional: false,
                skipTrailingNonNullable: true
            );
            return [ctx.SyntaxFactory.ExpressionStatement(ThrowArgumentNullException(nameofSourceAccess))];
        }

        // target.A = null;
        return _targetsToSetNull.Count == 0
            ? null
            : _targetsToSetNull.Select(x => ctx.SyntaxFactory.ExpressionStatement(x.BuildAssignment(targetAccess, NullLiteral())));
    }
}
