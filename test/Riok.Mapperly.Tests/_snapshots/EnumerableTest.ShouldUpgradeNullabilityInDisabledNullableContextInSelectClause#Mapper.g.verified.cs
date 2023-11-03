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
        if (source.Value is { } sourceValue)
        {
            target.Value = MapToDArray(sourceValue);
        }
        return target;
    }

    private global::D? MapToD(global::C? source)
    {
        if (source == null)
            return default;
        var target = new global::D();
        target.Value = source.Value;
        return target;
    }

    private global::D?[] MapToDArray(global::C[] source)
    {
        var target = new global::D?[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            target[i] = MapToD(source[i]);
        }
        return target;
    }
}
