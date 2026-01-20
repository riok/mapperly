namespace Riok.Mapperly.Helpers;

public static class DictionaryExtensions
{
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys)
    {
        foreach (var key in keys)
        {
            dict.Remove(key);
        }
    }
}
