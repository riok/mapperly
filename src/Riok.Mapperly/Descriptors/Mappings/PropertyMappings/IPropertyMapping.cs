using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a property mapping or a container of property mappings.
/// </summary>
public interface IPropertyMapping
{
    StatementSyntax Build(
        ExpressionSyntax sourceAccess,
        ExpressionSyntax targetAccess);
}
