using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// A default implementation of <see cref="IExistingTargetMapping"/>.
/// </summary>
public abstract class ExistingTargetMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : IExistingTargetMapping
{
    public ITypeSymbol SourceType { get; } = sourceType;

    public ITypeSymbol TargetType { get; } = targetType;

    public virtual bool CallableByOtherMappings => true;

    public virtual bool IsSynthetic => false;

    public MappingBodyBuildingPriority BodyBuildingPriority => MappingBodyBuildingPriority.Default;

    public abstract IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target);
}
