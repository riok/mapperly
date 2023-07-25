using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A tuple mapping creating the target instance via a tuple expression (eg. (A: 10, B: 20)).
/// </summary>
public interface INewValueTupleMapping : IMapping
{
    void AddConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping);
}
