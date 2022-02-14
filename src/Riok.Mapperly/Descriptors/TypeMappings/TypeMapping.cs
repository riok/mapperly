using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping to map from one type to another.
/// </summary>
[DebuggerDisplay("{GetType()}({SourceType.Name} => {TargetType.Name})")]
public abstract class TypeMapping
{
    protected TypeMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    public ITypeSymbol SourceType { get; }

    public ITypeSymbol TargetType { get; }

    /// <summary>
    /// Gets a value indicating if this mapping can be called / built by another mapping.
    /// This should be <c>true</c> for most mappings.
    /// </summary>
    public virtual bool CallableByOtherMappings => true;

    public abstract ExpressionSyntax Build(ExpressionSyntax source);
}
