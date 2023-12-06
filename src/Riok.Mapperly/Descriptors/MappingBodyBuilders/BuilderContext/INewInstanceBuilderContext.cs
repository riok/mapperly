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

    /// <summary>
    /// Maps case insensitive target root member names to their real case sensitive names.
    /// For example id => Id. The real name can then be used as key for <see cref="IMembersBuilderContext{T}.MemberConfigsByRootTargetName"/>.
    /// This allows resolving case insensitive configuration member names (eg. when mapping to constructor parameters).
    /// </summary>
    IReadOnlyDictionary<string, string> RootTargetNameCasingMapping { get; }
}
