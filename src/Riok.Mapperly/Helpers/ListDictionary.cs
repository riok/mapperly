namespace Riok.Mapperly.Helpers;

/// <summary>
/// A simple dictionary with multiple values per a key.
/// Not thread safe.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class ListDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly IReadOnlyList<TValue> _empty = [];
    private readonly Dictionary<TKey, List<TValue>> _data = new();

    public IReadOnlyList<TValue> GetOrEmpty(TKey key) => _data.GetValueOrDefault(key) ?? _empty;

    public bool ContainsKey(TKey key) => _data.ContainsKey(key);

    public void Add(TKey key, TValue value)
    {
        if (_data.TryGetValue(key, out var list))
        {
            list.Add(value);
        }
        else
        {
            _data.Add(key, [value]);
        }
    }

    public void Remove(TKey key) => _data.Remove(key);
}
