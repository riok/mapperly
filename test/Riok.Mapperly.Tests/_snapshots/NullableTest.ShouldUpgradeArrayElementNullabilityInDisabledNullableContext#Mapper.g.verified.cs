﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::B?[]? Map(global::A?[]? source)
    {
        if (source == null)
            return default;
        var target = new global::B?[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            target[i] = MapToB(source[i]);
        }
        return target;
    }

    private global::B? MapToB(global::A? source)
    {
        if (source == null)
            return default;
        var target = new global::B();
        return target;
    }
}