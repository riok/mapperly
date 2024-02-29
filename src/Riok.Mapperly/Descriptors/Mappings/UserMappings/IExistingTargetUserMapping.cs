using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A <see cref="IUserMapping"/> which is also a <see cref="IExistingTargetMapping"/>.
/// </summary>
public interface IExistingTargetUserMapping : IUserMapping, IExistingTargetMapping;
