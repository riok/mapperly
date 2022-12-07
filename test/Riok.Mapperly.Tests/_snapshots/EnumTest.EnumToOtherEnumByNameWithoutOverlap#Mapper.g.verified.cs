//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial E2 ToE1(E1 source)
    {
        return source switch
        {
            _ => throw new System.ArgumentOutOfRangeException(nameof(source)),
        };
    }
}