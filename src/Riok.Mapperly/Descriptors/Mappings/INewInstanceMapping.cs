using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping from one type to another new instance.
/// </summary>
public interface INewInstanceMapping : ITypeMapping
{
    /// <summary>
    /// Serializes the mapping as c# syntax.
    /// </summary>
    /// <param name="ctx">The build context.</param>
    /// <returns>The built syntax.</returns>
    ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
