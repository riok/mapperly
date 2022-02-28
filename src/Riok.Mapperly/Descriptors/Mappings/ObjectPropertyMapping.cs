using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a complex object mapping implemented in its own method.
/// Maps each property from the source to the target.
/// </summary>
public abstract class ObjectPropertyMapping : MethodMapping, IPropertyMappingContainer
{
    private readonly HashSet<IPropertyMapping> _mappings = new();

    protected ObjectPropertyMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    public void AddPropertyMapping(IPropertyMapping mapping)
        => _mappings.Add(mapping);

    public void AddPropertyMappings(IEnumerable<IPropertyMapping> mappings)
    {
        foreach (var mapping in mappings)
        {
            _mappings.Add(mapping);
        }
    }

    public bool HasPropertyMapping(IPropertyMapping mapping)
        => _mappings.Contains(mapping);

    internal IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source, ExpressionSyntax target)
        => _mappings.Select(x => x.Build(source, target));
}
