﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::System.Linq.IQueryable<global::B> Map(global::System.Linq.IQueryable<global::A> source)
    {
        return System.Linq.Queryable.Select(source, MapToExpression());
    }

    private global::System.Linq.Expressions.Expression<global::System.Func<global::A, global::B>> MapToExpression()
    {
#nullable disable
        return x => new global::B()
        {
            Values = global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Select(x.Values, x1 => (int)x1)),
        };
#nullable enable
    }
}