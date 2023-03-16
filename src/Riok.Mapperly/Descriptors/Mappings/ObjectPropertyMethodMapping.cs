using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A mapping from type to another by mapping each property.
/// A <see cref="MethodMapping"/> implementation of <see cref="IPropertyAssignmentTypeMapping"/>.
/// </summary>
public abstract class ObjectPropertyMethodMapping :
    MethodMapping,
    IPropertyAssignmentTypeMapping
{
    private readonly ObjectPropertyExistingTargetMapping _mapping;

    protected ObjectPropertyMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
        _mapping = new ObjectPropertyExistingTargetMapping(sourceType, targetType);
    }

    public bool HasPropertyMapping(IPropertyAssignmentMapping mapping)
        => _mapping.HasPropertyMapping(mapping);

    public void AddPropertyMapping(IPropertyAssignmentMapping mapping)
        => _mapping.AddPropertyMapping(mapping);

    public bool HasPropertyMappingContainer(IPropertyAssignmentMappingContainer container)
        => _mapping.HasPropertyMappingContainer(container);

    public void AddPropertyMappingContainer(IPropertyAssignmentMappingContainer container)
        => _mapping.AddPropertyMappingContainer(container);

    public IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
        => BuildBody(ctx, targetAccess);

    protected IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx, ExpressionSyntax target)
        => _mapping.Build(ctx, target);
}
