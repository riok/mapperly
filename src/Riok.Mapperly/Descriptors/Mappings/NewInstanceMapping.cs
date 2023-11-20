using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <inheritdoc cref="INewInstanceMapping"/>
[DebuggerDisplay("{GetType().Name}({SourceType} => {TargetType})")]
public abstract class NewInstanceMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : INewInstanceMapping
{
    public ITypeSymbol SourceType { get; } = sourceType;

    public ITypeSymbol TargetType { get; } = targetType;

    public virtual MappingBodyBuildingPriority BodyBuildingPriority => MappingBodyBuildingPriority.Default;

    /// <inheritdoc cref="INewInstanceMapping.CallableByOtherMappings"/>
    public virtual bool CallableByOtherMappings => true;

    /// <inheritdoc cref="INewInstanceMapping.IsSynthetic"/>
    public virtual bool IsSynthetic => false;

    public abstract ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
