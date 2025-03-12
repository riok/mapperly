using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Creates a composite mapping for existing target
/// <code>
/// OutputMapping(SourceMapping(source), TargetMapping(target));
/// </code>
/// </summary>
public class CompositeExistingTargetMapping(
    IExistingTargetMapping delegateMapping,
    INewInstanceMapping sourceMapping,
    INewInstanceMapping targetMapping
) : ExistingTargetMapping(sourceMapping.SourceType, targetMapping.TargetType)
{
    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var source = sourceMapping.Build(ctx);
        var target = targetMapping.Build(ctx.WithSource(targetAccess));
        return delegateMapping.Build(ctx.WithSource(source), target);
    }
}
