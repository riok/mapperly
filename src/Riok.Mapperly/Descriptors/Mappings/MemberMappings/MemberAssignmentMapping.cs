using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a member mapping including an assignment to a target member.
/// (e.g. target.A = source.B or target.A = "fooBar")
/// </summary>
[DebuggerDisplay("MemberAssignmentMapping({_sourceValue} => {_targetPath})")]
public class MemberAssignmentMapping(MemberPathSetter targetPath, ISourceValue sourceValue, MemberMappingInfo memberInfo)
    : IMemberAssignmentMapping,
        IHasUsedNames
{
    public MemberMappingInfo MemberInfo { get; } = memberInfo;

    private readonly ISourceValue _sourceValue = sourceValue;
    private readonly MemberPathSetter _targetPath = targetPath;

    public bool TryGetMemberAssignmentMappingContainer([NotNullWhen(true)] out IMemberAssignmentMappingContainer? container)
    {
        container = null;
        return false;
    }

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess) =>
        ctx.SyntaxFactory.SingleStatement(BuildExpression(ctx, targetAccess));

    public ExpressionSyntax BuildExpression(TypeMappingBuildContext ctx, ExpressionSyntax? targetAccess)
    {
        var mappedValue = _sourceValue.Build(ctx);

        // target.SetValue(source.Value); or target.Value = source.Value;
        return _targetPath.BuildAssignment(targetAccess, mappedValue);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (MemberAssignmentMapping)obj;
        return _sourceValue.Equals(other._sourceValue) && _targetPath.Equals(other._targetPath);
    }

    public override int GetHashCode() => HashCode.Combine(_sourceValue, _targetPath);

    public IEnumerable<string> ExtractUsedParameters() => UsedNamesHelpers.ExtractUsedName(_sourceValue);
}
