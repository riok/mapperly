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
            target.Value = MapToD(source.Value);
        }

        return target;
    }

    private global::D MapToD(global::C source)
    {
        var target = new global::D();
        target.Value = source.Value;
        return target;
    }
}
