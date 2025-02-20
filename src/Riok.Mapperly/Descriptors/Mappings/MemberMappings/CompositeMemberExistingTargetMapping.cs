using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Creates a composite mapping for existing target
/// <code>
/// OutputMapping(SourceMapping(source), TargetMapping(target));
/// </code>
/// </summary>
public class CompositeMemberExistingTargetMapping(
    IExistingTargetMapping delegateMapping,
    INewInstanceMapping sourceMapping,
    MemberPathGetter sourcePath,
    INewInstanceMapping targetMapping,
    MemberPathGetter targetPath,
    MemberMappingInfo memberInfo
) : IMemberAssignmentMapping
{
    public MemberMappingInfo MemberInfo { get; } = memberInfo;

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var sourcePathAccess = sourcePath.BuildAccess(ctx.Source);
        var source = sourceMapping.Build(ctx.WithSource(sourcePathAccess));

        var targetPathAccess = targetPath.BuildAccess(targetAccess);
        var target = targetMapping.Build(ctx.WithSource(targetPathAccess));

        return delegateMapping.Build(ctx.WithSource(source), target);
    }
}
