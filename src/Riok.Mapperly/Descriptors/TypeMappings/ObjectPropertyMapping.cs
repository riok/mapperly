using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a complex object mapping implemented in its own method.
/// Maps each property from the source to the target.
/// </summary>
public abstract class ObjectPropertyMapping : MethodMapping
{
    private readonly List<PropertyMapping> _propertyMappings = new();

    protected ObjectPropertyMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    public void AddPropertyMapping(PropertyMapping propertyMapping)
        => _propertyMappings.Add(propertyMapping);

    internal IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source, ExpressionSyntax target)
        => _propertyMappings.Select(x => x.Build(source, target));
}
