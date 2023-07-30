using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <inheritdoc cref="ITypeMapping"/>
[DebuggerDisplay("{GetType().Name}({SourceType} => {TargetType})")]
public abstract class TypeMapping : ITypeMapping
{
    protected TypeMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
        Parameters = ImmutableEquatableArray<MethodParameter>.Empty;
    }

    protected TypeMapping(ITypeSymbol sourceType, ITypeSymbol targetType, ImmutableEquatableArray<MethodParameter> parameters)
    {
        SourceType = sourceType;
        TargetType = targetType;
        Parameters = parameters;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }

    public ImmutableEquatableArray<MethodParameter> Parameters { get; }

    public virtual MappingBodyBuildingPriority BodyBuildingPriority => MappingBodyBuildingPriority.Default;

    /// <inheritdoc cref="ITypeMapping.CallableByOtherMappings"/>
    public virtual bool CallableByOtherMappings => true;

    /// <inheritdoc cref="ITypeMapping.IsSynthetic"/>
    public virtual bool IsSynthetic => false;

    public abstract ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
