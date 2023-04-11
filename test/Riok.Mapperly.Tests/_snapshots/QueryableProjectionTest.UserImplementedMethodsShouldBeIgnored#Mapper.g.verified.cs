//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial global::System.Linq.IQueryable<global::B> Map(global::System.Linq.IQueryable<global::A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new global::B() { StringValue = x.StringValue, NestedValue = new global::D() { Value = (int)x.NestedValue.Value } });
#nullable enable
    }

    private partial global::B MapToB(global::A source)
    {
        var target = new global::B();
        target.StringValue = source.StringValue;
        target.NestedValue = MapToD(source.NestedValue);
        return target;
    }
}
