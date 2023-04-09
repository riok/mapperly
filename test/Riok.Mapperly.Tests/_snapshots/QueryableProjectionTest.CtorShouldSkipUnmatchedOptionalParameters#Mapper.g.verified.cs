﻿//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial System.Linq.IQueryable<B> Map(global::System.Linq.IQueryable<global::A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new B(x.StringValue) { IntValue = x.IntValue });
#nullable enable
    }
}
