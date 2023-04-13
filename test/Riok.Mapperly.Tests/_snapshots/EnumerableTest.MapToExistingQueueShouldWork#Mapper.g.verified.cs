//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial void Map(global::System.Collections.Generic.List<global::A>? source, global::System.Collections.Generic.Queue<global::B> target)
    {
        if (source == null)
            return;
        foreach (var item in source)
        {
            target.Enqueue(MapToB(item));
        }
    }

    private global::B MapToB(global::A source)
    {
        var target = new global::B();
        target.Value = source.Value;
        return target;
    }
}
