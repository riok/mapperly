using System.Runtime.CompilerServices;

namespace Riok.Mapperly.Abstractions.ReferenceHandling.Internal;

/// <summary>
/// Defines methods to support the comparison of objects for reference equality.
/// </summary>
/// <typeparam name="T">The type of objects to compare.</typeparam>
internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
{
    // cannot use System.Collections.Generic.ReferenceEqualityComparer since it is not available in netstandard2.0

    /// <summary>
    /// A <see cref="ReferenceEqualityComparer{T}"/> instance.
    /// </summary>
    public static readonly IEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

    private ReferenceEqualityComparer() { }

    bool IEqualityComparer<T>.Equals(T? x, T? y) => ReferenceEquals(x, y);

    int IEqualityComparer<T>.GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}
