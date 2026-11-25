namespace Riok.Mapperly.Configuration;

/// <summary>
/// Represents a configuration object that can produce a reversed version of itself.
/// </summary>
/// <typeparam name="TData">The type of the reversed configuration.</typeparam>
public interface IReversible<out TData>
{
    /// <summary>
    /// Returns a reversed version of this configuration.
    /// </summary>
    TData Reverse();
}
