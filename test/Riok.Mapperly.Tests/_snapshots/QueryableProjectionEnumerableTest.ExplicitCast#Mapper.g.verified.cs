//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial System.Linq.IQueryable<B> Map(global::System.Linq.IQueryable<global::A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new B() { Values = System.Linq.Enumerable.Select(x.Values, x1 => (int)x1) });
#nullable enable
    }
}
