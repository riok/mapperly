using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a mapping which works by accessing an instance member on the source object
/// and then mapping the result to the target with an existing target mapper.
/// <code>
/// Map(source.Span, target);
/// </code>
/// </summary>
public class SourceObjectMemberDelegateExistingTargetMapping : ExistingTargetMapping
{
    private readonly string _memberName;
    private readonly IExistingTargetMapping _delegateMapping;

    public SourceObjectMemberDelegateExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        string memberName,
        IExistingTargetMapping delegateMapping
    )
        : base(sourceType, targetType)
    {
        _memberName = memberName;
        _delegateMapping = delegateMapping;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var member = MemberAccess(ctx.Source, _memberName);
        return _delegateMapping.Build(ctx.WithSource(member), target);
    }
}
