using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A <see cref="IMemberAssignmentMapping"/> which maps to an existing target instance.
/// </summary>
public class MemberExistingTargetMapping(
    IExistingTargetMapping delegateMapping,
    MemberPathGetter sourcePath,
    MemberPathGetter targetPath,
    MemberMappingInfo memberInfo
) : IMemberAssignmentMapping
{
    public MemberMappingInfo MemberInfo { get; } = memberInfo;

    public bool TryGetMemberAssignmentMappingContainer([NotNullWhen(true)] out IMemberAssignmentMappingContainer? container)
    {
        container = delegateMapping as IMemberAssignmentMappingContainer;
        return container != null;
    }

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var target = targetPath.BuildAccess(targetAccess);

        if (!sourcePath.MemberPath.IsAnyObjectReadPathNullable())
        {
            var source = sourcePath.BuildAccess(ctx.Source);
            return delegateMapping.Build(ctx.WithSource(source), target);
        }

        if (delegateMapping is NullDelegateExistingTargetMapping nullDelegateMapping && nullDelegateMapping.SourceType.IsNullable())
        {
            var nullSafeSource = sourcePath.BuildAccess(ctx.Source, nullConditional: true);
            var source = sourcePath.BuildAccess(ctx.Source, addValuePropertyOnNullable: true);
            return nullDelegateMapping.Build(ctx.WithSource(source), target, nullSafeSource);
        }

        if (delegateMapping.SourceType.IsNullable())
        {
            var nullSafeSource = sourcePath.BuildAccess(ctx.Source, nullConditional: true);
            return delegateMapping.Build(ctx.WithSource(nullSafeSource), target);
        }

        var notNullCondition = sourcePath.BuildNotNullCondition(ctx.Source, useNullConditionalAccess: true)!;
        var sourceAccess = sourcePath.BuildAccess(ctx.Source, addValuePropertyOnNullable: true);
        var body = delegateMapping.Build(ctx.AddIndentation().WithSource(sourceAccess), target).ToArray();
        return body.Length == 0 ? [] : [ctx.SyntaxFactory.If(notNullCondition, body)];
    }
}
