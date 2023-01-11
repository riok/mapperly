using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a property assignment mapping or a container of property assignment mappings.
/// </summary>
public interface IPropertyAssignmentMapping
{
    StatementSyntax Build(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetAccess);
}
