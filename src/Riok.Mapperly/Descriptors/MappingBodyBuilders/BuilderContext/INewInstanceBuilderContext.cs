using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// A <see cref="IMembersBuilderContext{T}"/> for mappings which create the target object via new ...().
/// </summary>
/// <typeparam name="T">The mapping type.</typeparam>
public interface INewInstanceBuilderContext<out T> : IMembersBuilderContext<T>
    where T : IMapping
{
    bool TryMatchParameter(IParameterSymbol parameter, [NotNullWhen(true)] out MemberMappingInfo? memberInfo);

    bool TryMatchInitOnlyMember(IMappableMember targetMember, [NotNullWhen(true)] out MemberMappingInfo? memberInfo);

    void AddConstructorParameterMapping(ConstructorParameterMapping mapping);

    void AddInitMemberMapping(MemberAssignmentMapping mapping);
}
