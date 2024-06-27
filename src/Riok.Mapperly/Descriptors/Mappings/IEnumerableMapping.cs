using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;

namespace Riok.Mapperly.Descriptors.Mappings;

public interface IEnumerableMapping : IMemberAssignmentTypeMapping
{
    CollectionInfos CollectionInfos { get; }

    void AddEnsureCapacity(EnsureCapacityInfo ensureCapacityInfo);
}
