//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B? Map(A? source)
    {
        if (source == null)
            return default;
        var target = new B();
        return target;
    }
}