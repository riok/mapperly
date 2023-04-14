using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A <see cref="IMemberAssignmentMapping"/> which maps to an existing target instance.
/// </summary>
public class MemberExistingTargetMapping : IMemberAssignmentMapping
{
    private readonly IExistingTargetMapping _delegateMapping;

    public MemberExistingTargetMapping(IExistingTargetMapping delegateMapping, MemberPath sourcePath, MemberPath targetPath)
    {
        _delegateMapping = delegateMapping;
        SourcePath = sourcePath;
        TargetPath = targetPath;
    }

    public MemberPath SourcePath { get; }

    public MemberPath TargetPath { get; }

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var source = SourcePath.BuildAccess(ctx.Source);
        var target = TargetPath.BuildAccess(targetAccess);
        return _delegateMapping.Build(ctx.WithSource(source), target);
    }
}
