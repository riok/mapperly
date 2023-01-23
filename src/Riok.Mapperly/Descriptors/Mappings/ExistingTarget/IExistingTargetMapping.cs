using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// A mapping which can be applied to an existing target type object instance.
/// </summary>
public interface IExistingTargetMapping : IMapping
{
    IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target);
}
