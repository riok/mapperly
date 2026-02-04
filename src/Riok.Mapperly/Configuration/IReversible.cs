namespace Riok.Mapperly.Configuration;

/// <summary>
/// Represents an object that can be reversed to produce a new instance of type <typeparamref name="TData"/>.
/// </summary>
/// <typeparam name="TData">The type of data produced when reversing. This type parameter is covariant.</typeparam>
public interface IReversible<out TData>
{
    /// <summary>
    /// Creates a reversed version of the current instance.
    /// </summary>
    /// <returns>A new instance of <typeparamref name="TData"/> that represents the reversed state.</returns>
    public TData Reverse();
}
