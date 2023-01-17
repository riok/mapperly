using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <inheritdoc cref="ITypeMapping"/>
[DebuggerDisplay("{GetType()}({SourceType.Name} => {TargetType.Name})")]
public abstract class TypeMapping : ITypeMapping
{
    protected TypeMapping(ITypeSymbol sourceType, ITypeSymbol targetType, RefKind targetRefKind = RefKind.None)
    {
        SourceType = sourceType;
        TargetType = targetType;
        TargetRefKind = targetRefKind;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }

    /// <inheritdoc cref="ITypeMapping.CallableByOtherMappings"/>
    public virtual bool CallableByOtherMappings => true;

    /// <inheritdoc cref="ITypeMapping.IsSynthetic"/>
    public virtual bool IsSynthetic => false;

    public RefKind TargetRefKind { get; }

    public abstract ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
