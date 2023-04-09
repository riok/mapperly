//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(global::A source)
    {
        var target = new B();
        if (source.Value != null)
        {
            foreach (var item in source.Value)
            {
                target.Value.Add((long)item);
            }
        }

        return target;
    }
}
