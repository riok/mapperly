//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B? Map(A? source)
    {
        if (source == null)
            return default;
        var target = new B();
        if (source.Value != null)
        {
            target.Value = MapToD(source.Value);
        }

        return target;
    }

    private D MapToD(C source)
    {
        var target = new D();
        target.Value = source.Value;
        return target;
    }
}