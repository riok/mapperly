//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(string? source)
    {
        return source == null ? throw new System.ArgumentNullException(nameof(source)) : B.Parse(source);
    }
}