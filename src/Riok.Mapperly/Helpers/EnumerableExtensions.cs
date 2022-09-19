namespace Riok.Mapperly.Helpers;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : notnull
    {
#nullable disable
        return enumerable.Where(x => x != null);
#nullable restore
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T>? comparer = null)
    {
        var set = new HashSet<T>(comparer);
        foreach (var item in enumerable)
        {
            set.Add(item);
        }

        return set;
    }

    public static IEnumerable<T> DistinctBy<T, TProp>(
        this IEnumerable<T> enumerable,
        Func<T, TProp> selector,
        IEqualityComparer<TProp>? equalityComparer = null)
    {
        var set = new HashSet<TProp>(equalityComparer);
        foreach (var item in enumerable)
        {
            if (set.Add(selector(item)))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<IReadOnlyCollection<T>> Chunk<T>(this IEnumerable<T> enumerable, Func<T, int, bool> shouldChunk)
    {
        var l = new List<T>();
        var i = 0;
        foreach (var item in enumerable)
        {
            l.Add(item);
            if (!shouldChunk(item, i++))
                continue;

            if (l.Count == 0)
                continue;

            yield return l;
            l = new();
        }

        if (l.Count != 0)
            yield return l;
    }

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> enumerable)
    {
        using var enumerator = enumerable.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        var previousItem = enumerator.Current;
        while (enumerator.MoveNext())
        {
            yield return previousItem;
            previousItem = enumerator.Current;
        }
    }

    public static TAccumulate AggregateWithPrevious<T, TAccumulate>(
        this IEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T?, T, TAccumulate> func)
    {
        var result = seed;
        T? prev = default;
        foreach (var element in source)
        {
            result = func(result, prev, element);
            prev = element;
        }

        return result;
    }
}
