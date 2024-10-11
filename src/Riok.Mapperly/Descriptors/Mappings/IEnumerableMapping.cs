using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.Capacity;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

public interface IEnumerableMapping : IMemberAssignmentTypeMapping
{
    CollectionInfos CollectionInfos { get; }

    void AddCapacitySetter(ICapacitySetter capacitySetter);
}
