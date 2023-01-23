//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B? Map(A? source)
    {
        if (source == null)
            return default;
        var target = new B();
        if (source.Value != null)
        {
            target.Value = MapToDArray(source.Value);
        }

        return target;
    }

    private D? MapToD(C? source)
    {
        if (source == null)
            return default;
        var target = new D();
        target.Value = source.Value;
        return target;
    }

    private D[] MapToDArray(C[] source)
    {
        var target = new D?[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            target[i] = MapToD(source[i]);
        }

        return target;
    }
}
