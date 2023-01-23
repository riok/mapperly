using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// A <see cref="IPropertyAssignmentMapping"/> which maps to an existing target instance.
/// </summary>
public class PropertyExistingTargetMapping : IPropertyAssignmentMapping
{
    private readonly IExistingTargetMapping _delegateMapping;

    public PropertyExistingTargetMapping(IExistingTargetMapping delegateMapping, PropertyPath sourcePath, PropertyPath targetPath)
    {
        _delegateMapping = delegateMapping;
        SourcePath = sourcePath;
        TargetPath = targetPath;
    }

    public PropertyPath SourcePath { get; }

    public PropertyPath TargetPath { get; }

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var source = SourcePath.BuildAccess(ctx.Source);
        var target = TargetPath.BuildAccess(targetAccess);
        return _delegateMapping.Build(ctx.WithSource(source), target);
    }
}
