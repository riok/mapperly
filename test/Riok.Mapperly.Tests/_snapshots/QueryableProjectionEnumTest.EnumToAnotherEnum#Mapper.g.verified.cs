//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new B() { Value = (D)x.Value });
#nullable enable
    }
}