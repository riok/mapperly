using System.Diagnostics.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

public static class DictionaryExtensions
{
    public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!dict.TryGetValue(key, out value))
            return false;

        dict.Remove(key);
        return true;
    }

    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys)
    {
        foreach (var key in keys)
        {
            dict.Remove(key);
        }
    }
}
