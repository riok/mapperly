//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial global::System.Linq.IQueryable<global::B> Map(global::System.Linq.IQueryable<global::A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new global::B() { StringValue = x.StringValue ?? "" });
#nullable enable
    }
}
