//HintName: Mapper.g.cs
#nullable enable
public sealed class Mapper : IMapper
{
    public static readonly IMapper Instance = new Mapper();
    public B Map(string? source)
    {
        return source == null ? throw new System.ArgumentNullException(nameof(source)) : B.Parse(source);
    }
}