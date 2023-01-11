using System.Diagnostics.CodeAnalysis;

namespace Riok.Mapperly.Abstractions.ReferenceHandling;

/// <summary>
/// A reference handler can store and resolve references
/// of mapping target objects.
/// </summary>
public interface IReferenceHandler
{
    /// <summary>
    /// Before an object is created by Mapperly this method is called.
    /// It can attempt to resolve existing target object instances based on the source object instance.
    /// If <c>false</c> is returned, Mapperly creates a new instance of the target class.
    /// If <c>true</c> is returned, target has to be non-null.
    /// Mapperly then uses the target instance.
    /// </summary>
    /// <param name="source">The source object instance.</param>
    /// <param name="target">The resolved target object instance or <c>null</c> if none could be resolved.</param>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The target object type.</typeparam>
    /// <returns></returns>
    bool TryGetReference<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget? target)
        where TSource : notnull
        where TTarget : notnull;

    /// <summary>
    /// Stores the created target instance.
    /// Called by Mapperly just after a new target object instance is created.
    /// </summary>
    /// <param name="source">The source object instance.</param>
    /// <param name="target">The target object instance.</param>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    void SetReference<TSource, TTarget>(TSource source, TTarget target)
        where TSource : notnull
        where TTarget : notnull;
}
