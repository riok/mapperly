﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial TTarget Map<TSource, TTarget, TSource2, TTarget2>(TSource source) where TSource : global::System.Linq.IQueryable<TSource2> where TTarget : global::System.Linq.IQueryable<TTarget2>
    {
        return source switch
        {
            global::System.Linq.IQueryable<global::A> x when typeof(TTarget).IsAssignableFrom(typeof(global::System.Linq.IQueryable<global::B>)) => (TTarget)(object)MapToB(x),
            global::System.Linq.IQueryable<global::C> x when typeof(TTarget).IsAssignableFrom(typeof(global::System.Linq.IQueryable<global::D>)) => (TTarget)(object)MapToD(x),
            _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
        };
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::System.Linq.IQueryable<global::B> MapToB(global::System.Linq.IQueryable<global::A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new global::B(x.Value));
#nullable enable
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::System.Linq.IQueryable<global::D> MapToD(global::System.Linq.IQueryable<global::C> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new global::D(x.Value1));
#nullable enable
    }
}