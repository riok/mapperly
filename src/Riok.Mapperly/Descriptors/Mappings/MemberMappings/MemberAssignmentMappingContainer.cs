using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// A default implementation for <see cref="IMemberAssignmentMappingContainer"/>.
/// </summary>
public abstract class MemberAssignmentMappingContainer(IMemberAssignmentMappingContainer? parent = null) : IMemberAssignmentMappingContainer
{
    private readonly HashSet<IMemberAssignmentMapping> _delegateMappings = [];
    private readonly HashSet<IMemberAssignmentMappingContainer> _childContainers = [];
    private readonly List<IAssignmentMappings> _mappings = [];

    public virtual IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess) =>
        _mappings.SelectMany(x => x.Build(ctx, targetAccess));

    public void AddMemberMappingContainer(IMemberAssignmentMappingContainer container)
    {
        if (HasMemberMappingContainer(container))
            return;

        _childContainers.Add(container);
        _mappings.Add(container);
    }

    public bool HasMemberMappingContainer(IMemberAssignmentMappingContainer container) =>
        _childContainers.Contains(container) || parent?.HasMemberMappingContainer(container) == true;

    public void AddMemberMapping(IMemberAssignmentMapping mapping)
    {
        if (HasMemberMapping(mapping))
            return;

        _delegateMappings.Add(mapping);
        _mappings.Add(mapping);
    }

    public bool HasMemberMapping(IMemberAssignmentMapping mapping) =>
        _delegateMappings.Contains(mapping) || parent?.HasMemberMapping(mapping) == true;
}
