//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial void Map(global::System.Collections.Generic.List<global::A>? source, global::RepeatedField<global::B> target)
    {
        if (source == null)
            return;
        foreach (var item in source)
        {
            target.Add(MapToB(item));
        }
    }

    private B MapToB(global::A source)
    {
        var target = new B();
        target.Value = source.Value;
        return target;
    }
}
