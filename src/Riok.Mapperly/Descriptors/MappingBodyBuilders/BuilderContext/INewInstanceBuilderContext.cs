using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// A <see cref="IMembersBuilderContext{T}"/> for mappings which create the target object via new().
/// </summary>
/// <typeparam name="T">The mapping type.</typeparam>
public interface INewInstanceBuilderContext<out T> : IMembersBuilderContext<T>
    where T : IMapping
{
    void AddConstructorParameterMapping(ConstructorParameterMapping mapping);

    void AddInitMemberMapping(MemberAssignmentMapping mapping);
}
