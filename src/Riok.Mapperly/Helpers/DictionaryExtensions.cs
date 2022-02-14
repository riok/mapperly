namespace Riok.Mapperly.Helpers;

public static class DictionaryExtensions
{
    public static bool Remove<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        out TValue value)
    {
        if (!dict.TryGetValue(key, out value))
            return false;

        dict.Remove(key);
        return true;
    }

    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Func<TValue> valueProvider)
    {
        return dict.TryGetValue(key, out var value)
            ? value
            : dict[key] = valueProvider();
    }
}
