//HintName: MyMapperImpl.g.cs
#nullable enable
public sealed class MyMapperImpl : MyMapper
{
    public static readonly MyMapper Instance = new MyMapperImpl();
    public override B Map(A source)
    {
        var target = new B();
        target.Value = MyMapping(source.Value);
        target.Value2 = ((BaseMapper2)this).MyMapping2(source.Value2);
        target.Value3 = ((BaseMapper3)this).MyMapping3(source.Value3);
        return target;
    }
}