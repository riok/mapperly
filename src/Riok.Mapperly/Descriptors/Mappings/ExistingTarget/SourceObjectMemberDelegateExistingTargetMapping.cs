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
public class SourceObjectMemberDelegateExistingTargetMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    string memberName,
    IExistingTargetMapping delegateMapping
) : ExistingTargetMapping(sourceType, targetType)
{
    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var member = MemberAccess(ctx.Source, memberName);
        return delegateMapping.Build(ctx.WithSource(member), target);
    }
}
