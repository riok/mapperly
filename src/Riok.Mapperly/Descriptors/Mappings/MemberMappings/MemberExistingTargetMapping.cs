using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A <see cref="IMemberAssignmentMapping"/> which maps to an existing target instance.
/// </summary>
public class MemberExistingTargetMapping(IExistingTargetMapping delegateMapping, GetterMemberPath sourcePath, GetterMemberPath targetPath)
    : IMemberAssignmentMapping
{
    public GetterMemberPath SourcePath { get; } = sourcePath;

    public MemberPath TargetPath => targetPath;

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var source = SourcePath.BuildAccess(ctx.Source);
        var target = targetPath.BuildAccess(targetAccess);
        return delegateMapping.Build(ctx.WithSource(source), target);
    }
}
