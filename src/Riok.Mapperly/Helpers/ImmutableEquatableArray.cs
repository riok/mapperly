// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Riok.Mapperly.Helpers;

/// <summary>
/// Provides an immutable list implementation which implements sequence equality.
/// </summary>
public sealed class ImmutableEquatableArray<T>(IEnumerable<T> values) : IEquatable<ImmutableEquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    private readonly T[] _values = values.ToArray();
    public T this[int index] => _values[index];
    public int Count => _values.Length;

    public bool Equals(ImmutableEquatableArray<T>? other) => other != null && ((ReadOnlySpan<T>)_values).SequenceEqual(other._values);

    public override bool Equals(object? obj) => obj is ImmutableEquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in _values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    public Enumerator GetEnumerator() => new Enumerator(_values);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    public struct Enumerator
    {
        private readonly T[] _values;
        private int _index;

        internal Enumerator(T[] values)
        {
            _values = values;
            _index = -1;
        }

        public bool MoveNext()
        {
            var newIndex = _index + 1;

            if ((uint)newIndex >= (uint)_values.Length)
                return false;

            _index = newIndex;
            return true;
        }

        public readonly T Current => _values[_index];
    }
}

public static class ImmutableEquatableArray
{
    public static ImmutableEquatableArray<T> ToImmutableEquatableArray<T>(this IEnumerable<T> values)
        where T : IEquatable<T> => new(values);
}
