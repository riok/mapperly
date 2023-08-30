﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::B? Map(global::A? source)
    {
        if (source == null)
            return default;
        var target = new global::B();
        if (source.Value != null)
        {
            target.Value = MapToIReadOnlyCollection(source.Value);
        }
        return target;
    }

    private global::System.Collections.Generic.IReadOnlyCollection<string?> MapToIReadOnlyCollection(int[] source)
    {
        var target = new string?[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            target[i] = source[i].ToString();
        }
        return target;
    }
}