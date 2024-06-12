using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// A <see cref="IMembersBuilderContext{T}"/> for mappings which create the target object
/// via a tuple expression (eg. (A: source.A, B: MapToB(source.B))).
/// </summary>
/// <typeparam name="T">The mapping type.</typeparam>
public interface INewValueTupleBuilderContext<out T> : IMembersBuilderContext<T>
    where T : IMapping
{
    bool TryMatchTupleElement(IFieldSymbol member, [NotNullWhen(true)] out MemberMappingInfo? memberInfo);

    void AddTupleConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping);
}
