//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial global::B Map(global::A source)
    {
        var target = new global::B();
        if (source.Value != null && target.Value != null)
        {
            foreach (var item in source.Value)
            {
                target.Value.Add((long)item);
            }
        }

        return target;
    }
}
