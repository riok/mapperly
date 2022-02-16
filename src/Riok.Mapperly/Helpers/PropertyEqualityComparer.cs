namespace Riok.Mapperly.Helpers;

public class PropertyEqualityComparer<T, TValue> : IEqualityComparer<T>
{
    private readonly Func<T, TValue> _valueSelector;
    private readonly IEqualityComparer<TValue> _valueComparer;

    public PropertyEqualityComparer(Func<T, TValue> valueSelector, IEqualityComparer<TValue> valueComparer)
    {
        _valueSelector = valueSelector;
        _valueComparer = valueComparer;
    }

    public bool Equals(T x, T y)
        => _valueComparer.Equals(Value(x), Value(y));

    public int GetHashCode(T obj)
        => _valueComparer.GetHashCode(Value(obj));

    private TValue Value(T obj)
        => _valueSelector(obj);
}
