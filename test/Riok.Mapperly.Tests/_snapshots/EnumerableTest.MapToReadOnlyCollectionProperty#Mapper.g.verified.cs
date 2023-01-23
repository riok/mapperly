//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(A source)
    {
        var target = new B();
        foreach (var item in source.Value)
        {
            target.Value.Add((long)item);
        }

        return target;
    }
}