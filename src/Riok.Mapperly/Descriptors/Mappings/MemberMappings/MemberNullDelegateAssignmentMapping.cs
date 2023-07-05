using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// a member mapping container, which performs a null check before the mappings.
/// </summary>
[DebuggerDisplay("MemberNullDelegateAssignmentMapping({_nullConditionalSourcePath} != null)")]
public class MemberNullDelegateAssignmentMapping : MemberAssignmentMappingContainer
{
    private readonly MemberPath _nullConditionalSourcePath;
    private readonly bool _throwInsteadOfConditionalNullMapping;

    public MemberNullDelegateAssignmentMapping(
        MemberPath nullConditionalSourcePath,
        IMemberAssignmentMappingContainer parent,
        bool throwInsteadOfConditionalNullMapping
    )
        : base(parent)
    {
        _nullConditionalSourcePath = nullConditionalSourcePath;
        _throwInsteadOfConditionalNullMapping = throwInsteadOfConditionalNullMapping;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        // if (source.Value != null)
        //   target.Value = Map(Source.Name);
        // else
        //   throw ...
        var sourceNullConditionalAccess = _nullConditionalSourcePath.BuildAccess(ctx.Source, true, true, true);
        var nameofSourceAccess = _nullConditionalSourcePath.BuildAccess(ctx.Source, true, false, true);
        var condition = IsNotNull(sourceNullConditionalAccess);
        var elseClause = _throwInsteadOfConditionalNullMapping
            ? ElseClause(Block(ExpressionStatement(ThrowArgumentNullException(nameofSourceAccess))))
            : null;

#if !ROSLYN3_10_OR_GREATER
        // for old roslyn version (<= 3.10), ElseClauseSyntax param is not nullable:
        // IfStatement(ExpressionSyntax condition, StatementSyntax statement, ElseClauseSyntax @else)
        return new[] { IfStatement(condition, Block(base.Build(ctx, targetAccess)), elseClause!), };
#else
        return new[] { IfStatement(condition, Block(base.Build(ctx, targetAccess)), elseClause), };
#endif
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
