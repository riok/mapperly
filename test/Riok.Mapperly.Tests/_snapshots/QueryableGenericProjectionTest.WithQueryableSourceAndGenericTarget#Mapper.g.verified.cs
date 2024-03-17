﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    public partial global::System.Linq.IQueryable<TTarget> ProjectTo<TTarget>(global::System.Linq.IQueryable source)
    {
        return source switch
        {
            global::System.Linq.IQueryable<global::A> x when typeof(TTarget).IsAssignableFrom(typeof(global::B)) => (global::System.Linq.IQueryable<TTarget>)(object)ProjectAToB(x),
            null => throw new System.ArgumentNullException(nameof(source)),
            _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(global::System.Linq.IQueryable<TTarget>)} as there is no known type mapping", nameof(source)),
        };
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::System.Linq.IQueryable<global::B> ProjectAToB(global::System.Linq.IQueryable<global::A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new global::B()
        {
            Value = x.Value,
        });
#nullable enable
    }
}