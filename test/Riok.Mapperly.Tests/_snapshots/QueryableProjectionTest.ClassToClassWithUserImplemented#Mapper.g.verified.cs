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
            StringValue = x.StringValue,
            NestedValue = MapToD(x.NestedValue),
        };
#nullable enable
    }
}