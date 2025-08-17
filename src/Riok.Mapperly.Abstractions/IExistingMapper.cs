namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Optional contract for mappers that map into an existing destination instance.
/// Used for DI-based composition in existing-target member mappings.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IExistingMapper<in TSource, in TDestination>
{
    /// <summary>
    /// Maps the source to the destination instance.
    /// </summary>
    /// <param name="source">The source instance.</param>
    /// <param name="destination">The destination instance to map into.</param>
    void Map(TSource source, TDestination destination);
}
