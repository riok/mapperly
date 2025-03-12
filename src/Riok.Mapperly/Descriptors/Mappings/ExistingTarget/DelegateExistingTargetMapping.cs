using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a mapping which works by invoking existing target mapper.
/// </summary>
public class DelegateExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType, IExistingTargetMapping delegateMapping)
    : ExistingTargetMapping(sourceType, targetType)
{
    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target) =>
        delegateMapping.Build(ctx, target);
}
