//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial void Map(A? source, B? target)
    {
        if (source == null || target == null)
            return;
        target.StringValue = source.StringValue;
    }
}