namespace Riok.Mapperly.Helpers;

public static class HashSetExtensions
{
    public static void AddRange<TItem>(this HashSet<TItem> hashSet, IEnumerable<TItem> items)
    {
        foreach (var key in items)
        {
            hashSet.Add(key);
        }
    }
}
