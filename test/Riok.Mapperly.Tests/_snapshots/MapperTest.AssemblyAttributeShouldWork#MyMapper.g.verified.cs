//HintName: MyMapper.g.cs
// <auto-generated />
#nullable enable
public partial class MyMapper
{
    private partial global::E2 Map(global::E1 source)
    {
        return source switch
        {
            global::E1.value1 => global::E2.Value1,
            _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E1 is not supported"),
        };
    }
}
