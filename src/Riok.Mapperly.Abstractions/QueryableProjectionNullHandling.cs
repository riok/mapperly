namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Controls how <c>null</c> values are handled in <see cref="System.Linq.IQueryable{T}"/> projection mappings.
/// </summary>
public enum QueryableProjectionNullHandling
{
    /// <summary>
    /// Emits null-safe projections based on the nullable reference type annotations (default).
    /// </summary>
    NullSafe = 0,

    /// <summary>
    /// Skips null handling in queryable projections, even when the source may be <c>null</c>.
    /// Intended for relational providers (e.g. EF Core) where the projection is translated to SQL
    /// and the lambda is never executed. Using this with in-memory queryables may cause a <see cref="System.NullReferenceException"/>.
    /// </summary>
    Ignore = 1,
}
