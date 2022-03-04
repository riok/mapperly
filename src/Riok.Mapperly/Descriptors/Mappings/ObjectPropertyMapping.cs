using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a complex object mapping implemented in its own method.
/// Maps each property from the source to the target.
/// </summary>
public abstract class ObjectPropertyMapping : MethodMapping, IPropertyAssignmentMappingContainer
{
    private readonly HashSet<IPropertyAssignmentMapping> _mappings = new();

    protected ObjectPropertyMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    public void AddPropertyMapping(IPropertyAssignmentMapping mapping)
        => _mappings.Add(mapping);

    public void AddPropertyMappings(IEnumerable<IPropertyAssignmentMapping> mappings)
    {
        foreach (var mapping in mappings)
        {
            _mappings.Add(mapping);
        }
    }

    public bool HasPropertyMapping(IPropertyAssignmentMapping mapping)
        => _mappings.Contains(mapping);

    protected IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source, ExpressionSyntax target)
        => _mappings.Select(x => x.Build(source, target));
}
