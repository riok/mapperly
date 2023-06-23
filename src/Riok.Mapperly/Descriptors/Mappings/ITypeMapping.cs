using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping from one type to another.
/// </summary>
public interface ITypeMapping : IMapping
{
    /// <summary>
    /// Gets a value indicating if this mapping can be called / built by another mapping.
    /// This should be <c>true</c> for most mappings.
    /// </summary>
    bool CallableByOtherMappings { get; }

    /// <summary>
    /// Gets a value indicating whether this mapping produces any code or can be omitted completely (eg. direct assignments or delegate mappings).
    /// </summary>
    bool IsSynthetic { get; }

    ExpressionSyntax Build(TypeMappingBuildContext ctx);

    void AddParameters(MethodParameter[] parameters);
}
