//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial void Map(System.Collections.Generic.List<A>? source, RepeatedField<B> target)
    {
        if (source == null)
            return;
        foreach (var item in source)
        {
            target.Add(MapToB(item));
        }
    }

    private B MapToB(A source)
    {
        var target = new B();
        target.Value = source.Value;
        return target;
    }
}