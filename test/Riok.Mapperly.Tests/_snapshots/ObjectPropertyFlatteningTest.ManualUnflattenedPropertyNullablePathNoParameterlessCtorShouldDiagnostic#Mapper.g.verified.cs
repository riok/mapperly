//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(global::A source)
    {
        var target = new B();
        target.Value.Id = source.MyValueId;
        return target;
    }
}
