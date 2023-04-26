//HintName: MyMapper.g.cs
#nullable enable
public partial class MyMapper
{
    public partial global::B Map(global::A s)
    {
        var target = new global::B(StaticMapper(s.Value));
        return target;
    }
}