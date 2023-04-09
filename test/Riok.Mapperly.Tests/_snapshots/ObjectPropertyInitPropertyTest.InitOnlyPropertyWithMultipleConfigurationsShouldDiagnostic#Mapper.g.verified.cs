//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(global::A source)
    {
        var target = new B()
        {
            StringValue = source.StringValue2
        };
        return target;
    }
}
