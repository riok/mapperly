using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// A default implementation for <see cref="IPropertyAssignmentMappingContainer"/>.
/// </summary>
public abstract class PropertyAssignmentMappingContainer : IPropertyAssignmentMappingContainer
{
    private readonly HashSet<IPropertyAssignmentMapping> _delegateMappings = new();
    private readonly HashSet<IPropertyAssignmentMappingContainer> _childContainers = new();
    private readonly IPropertyAssignmentMappingContainer? _parent;

    protected PropertyAssignmentMappingContainer(IPropertyAssignmentMappingContainer? parent = null)
    {
        _parent = parent;
    }

    public virtual IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess)
    {
        var childContainerStatements = _childContainers.SelectMany(x => x.Build(ctx, targetAccess));
        var mappings = _delegateMappings.SelectMany(m => m.Build(ctx, targetAccess));
        return childContainerStatements.Concat(mappings);
    }

    public void AddPropertyMappingContainer(IPropertyAssignmentMappingContainer container)
    {
        if (!HasPropertyMappingContainer(container))
        {
            _childContainers.Add(container);
        }
    }

    public bool HasPropertyMappingContainer(IPropertyAssignmentMappingContainer container)
        => _childContainers.Contains(container) || _parent?.HasPropertyMappingContainer(container) == true;

    public void AddPropertyMapping(IPropertyAssignmentMapping mapping)
    {
        if (!HasPropertyMapping(mapping))
        {
            _delegateMappings.Add(mapping);
        }
    }

    public bool HasPropertyMapping(IPropertyAssignmentMapping mapping)
        => _delegateMappings.Contains(mapping) || _parent?.HasPropertyMapping(mapping) == true;
}
