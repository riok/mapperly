using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A <see cref="IMemberAssignmentMapping"/> which maps to an existing target instance.
/// </summary>
public class MemberExistingTargetMapping : IMemberAssignmentMapping
{
    private readonly IExistingTargetMapping _delegateMapping;
    private readonly GetterMemberPath _targetPath;

    public MemberExistingTargetMapping(IExistingTargetMapping delegateMapping, GetterMemberPath sourcePath, GetterMemberPath targetPath)
    {
        _delegateMapping = delegateMapping;
        SourcePath = sourcePath;
        _targetPath = targetPath;
    }

    public GetterMemberPath SourcePath { get; }

    public MemberPath TargetPath => _targetPath;

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var source = SourcePath.BuildAccess(ctx.Source);
        var target = _targetPath.BuildAccess(targetAccess);
        return _delegateMapping.Build(ctx.WithSource(source), target);
    }
}
