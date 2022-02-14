//HintName: MyMapper.g.cs
#nullable enable
public sealed class MyMapper : IMapper
{
    public static readonly IMapper Instance = new MyMapper();
    public int Map(string source)
    {
        return int.Parse(source);
    }
}