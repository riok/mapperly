﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::System.Collections.Generic.IReadOnlyCollection<global::B?>? Map(global::System.Collections.Generic.IEnumerable<global::A?>? source)
    {
        return source == null ? default : global::System.Linq.Enumerable.ToList(global::System.Linq.Enumerable.Select(source, x => MapToB(x)));
    }

    private global::B? MapToB(global::A? source)
    {
        if (source == null)
            return default;
        var target = new global::B();
        return target;
    }
}