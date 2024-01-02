using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// A default implementation of <see cref="IExistingTargetMapping"/>.
/// </summary>
public abstract class ExistingTargetMapping : IExistingTargetMapping
{
    protected ExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        Debug.Assert(sourceType.IsNullableUpgraded());
        Debug.Assert(targetType.IsNullableUpgraded());

        SourceType = sourceType;
        TargetType = targetType;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }

    public virtual bool CallableByOtherMappings => true;

    public virtual bool IsSynthetic => false;

    public MappingBodyBuildingPriority BodyBuildingPriority => MappingBodyBuildingPriority.Default;

    public abstract IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target);
}
