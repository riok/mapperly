//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial global::B Map(global::A source)
    {
        var target = new global::B()
        {
            StringValue = source.StringValue2
        };
        return target;
    }
}
