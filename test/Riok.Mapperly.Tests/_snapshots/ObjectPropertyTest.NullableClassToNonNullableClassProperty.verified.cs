//HintName: Mapper.g.cs
#nullable enable
public sealed class Mapper : IMapper
{
    public static readonly IMapper Instance = new Mapper();
    public B Map(A source)
    {
        var target = new B();
        target.Value = source.Value == null ? new D() : MapToD(source.Value);
        return target;
    }

    private D MapToD(C source)
    {
        var target = new D();
        target.V = source.V;
        return target;
    }
}