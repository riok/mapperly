namespace Riok.Mapperly.Helpers;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : struct
    {
#nullable disable
        return enumerable.Where(x => x != null).Select(x => x.Value);
#nullable restore
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : notnull
    {
#nullable disable
        return enumerable.Where(x => x != null);
#nullable restore
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
        TAccumulate? seed,
        Func<TAccumulate?, T?, T, TAccumulate> func
    )
    {
        var result = seed;
        T? prev = default;
        foreach (var element in source)
        {
            result = func(result, prev, element);
            prev = element;
        }

        return result ?? throw new InvalidOperationException("Aggregation was not initialized");
    }
}
