namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Simple mapping interface to allow DI based composition for nested mappings.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapper<in TSource, out TDestination>
{
    /// <summary>
    /// Maps the provided <paramref name="source"/> to a new <typeparamref name="TDestination"/> instance.
    /// </summary>
    TDestination Map(TSource source);
}
