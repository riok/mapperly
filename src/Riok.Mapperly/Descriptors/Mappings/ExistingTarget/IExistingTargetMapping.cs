using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// A mapping which can be applied to an existing target type object instance.
/// </summary>
public interface IExistingTargetMapping : ITypeMapping
{
    /// <summary>
    /// Serializes the existing target mapping as c# syntax.
    /// </summary>
    /// <param name="ctx">The build context.</param>
    /// <param name="target">The target of the existing target mapping.</param>
    /// <returns>The built syntax.</returns>
    IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target);
}
