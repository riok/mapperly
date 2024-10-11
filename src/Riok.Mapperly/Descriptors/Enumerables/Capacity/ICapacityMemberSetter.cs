using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Enumerables.Capacity;

/// <summary>
/// Sets the capacity of a collection to the provided count.
/// </summary>
public interface ICapacityMemberSetter : IMemberSetter
{
    IMappableMember? TargetCapacity { get; }
}
