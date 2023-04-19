//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    public partial global::B Map(global::A source)
    {
        var target = new global::B();
        target.Value = Map(source.Value);
        return target;
    }

    private partial global::BIntf Map(global::AIntf src)
    {
        return src switch
        {
            global::AImpl1 x => MapToBImpl1(x),
            global::AImpl2 x => MapToBImpl2(x),
            _ => throw new System.ArgumentException($"Cannot map {src.GetType()} to BIntf as there is no known derived type mapping", nameof(src)),
        };
    }

    private global::BImpl1 MapToBImpl1(global::AImpl1 source)
    {
        var target = new global::BImpl1();
        target.BaseValue = source.BaseValue;
        target.Value1 = source.Value1;
        return target;
    }

    private global::BImpl2 MapToBImpl2(global::AImpl2 source)
    {
        var target = new global::BImpl2();
        target.BaseValue = source.BaseValue;
        target.Value2 = source.Value2;
        return target;
    }
}