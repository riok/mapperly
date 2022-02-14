//HintName: Mapper.g.cs
#nullable enable
public sealed class Mapper : IMapper
{
    public static readonly IMapper MyMapperInstance = new Mapper();
    public int Map(string source)
    {
        return int.Parse(source);
    }
}