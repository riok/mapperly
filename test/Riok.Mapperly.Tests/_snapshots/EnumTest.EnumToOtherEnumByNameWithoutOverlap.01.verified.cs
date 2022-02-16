//HintName: Mapper.g.cs
#nullable enable
public sealed class Mapper : IMapper
{
    public static readonly IMapper Instance = new Mapper();
    public E2 ToE1(E1 source)
    {
        return source switch
        {
            _ => throw new System.ArgumentOutOfRangeException(nameof(source)),
        };
    }
}