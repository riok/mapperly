//HintName: Mapper.g.cs
#nullable enable
public sealed class Mapper : IMapper
{
    public static readonly IMapper Instance = new Mapper();
    public B Map(A source)
    {
        var target = new B();
        target.Value = ((BaseMapper)this).MyMapping(source.Value);
        target.Value2 = ((BaseMapper2)this).MyMapping2(source.Value2);
        target.Value3 = ((BaseMapper3)this).MyMapping3(source.Value3);
        return target;
    }
}