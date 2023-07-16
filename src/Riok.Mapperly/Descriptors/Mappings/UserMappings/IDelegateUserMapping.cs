namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A delegated user mapping.
/// </summary>
public interface IDelegateUserMapping : IUserMapping
{
    /// <summary>
    /// Gets the delegate mapping or <c>null</c> if none is set (yet).
    /// </summary>
    INewInstanceMapping? DelegateMapping { get; }

    /// <summary>
    /// Sets the delegate mapping.
    /// </summary>
    /// <param name="mapping">The mapping.</param>
    void SetDelegateMapping(INewInstanceMapping mapping);
}
