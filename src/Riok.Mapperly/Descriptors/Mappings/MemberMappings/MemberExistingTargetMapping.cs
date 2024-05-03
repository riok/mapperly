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
    public GetterMemberPath SourceGetter { get; } = sourcePath;

    public NonEmptyMemberPath TargetPath => (NonEmptyMemberPath)targetPath.MemberPath;

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var source = SourceGetter.BuildAccess(ctx.Source);
        var target = targetPath.BuildAccess(targetAccess);
        return delegateMapping.Build(ctx.WithSource(source), target);
    }
}
