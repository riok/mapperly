using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a container of several <see cref="IPropertyAssignmentMapping"/>.
/// </summary>
public interface IPropertyAssignmentMappingContainer
{
    bool HasPropertyMapping(IPropertyAssignmentMapping mapping);

    void AddPropertyMapping(IPropertyAssignmentMapping mapping);

    bool HasPropertyMappingContainer(IPropertyAssignmentMappingContainer container);

    void AddPropertyMappingContainer(IPropertyAssignmentMappingContainer container);

    IEnumerable<StatementSyntax> Build(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetAccess);
}
