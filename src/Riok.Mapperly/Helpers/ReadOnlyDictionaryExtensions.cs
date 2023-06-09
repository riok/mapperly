namespace Riok.Mapperly.Helpers;

internal static class ReadOnlyDictionaryExtensions
{
    public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
    {
        dict.TryGetValue(key, out var value);
        return value;
    }
}
