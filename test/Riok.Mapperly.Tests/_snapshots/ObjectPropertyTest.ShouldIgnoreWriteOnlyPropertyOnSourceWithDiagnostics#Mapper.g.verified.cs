//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(global::A source)
    {
        var target = new B();
        target.StringValue = source.StringValue;
        return target;
    }
}
