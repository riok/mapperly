namespace Riok.Mapperly.Helpers;

public static class EnumerableExtensions
{
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
    {
        var set = new HashSet<T>();
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

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : class
#nullable disable
        => enumerable.Where(x => x != null);
#nullable enable
}
