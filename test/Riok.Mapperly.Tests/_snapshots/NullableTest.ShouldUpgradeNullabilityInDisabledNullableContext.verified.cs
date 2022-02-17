//HintName: Mapper.g.cs
#nullable enable
public sealed class Mapper : IMapper
{
    public static readonly IMapper Instance = new Mapper();
    public B? Map(A? source)
    {
        if (source == null)
            return default;
        var target = new B();
        return target;
    }
}