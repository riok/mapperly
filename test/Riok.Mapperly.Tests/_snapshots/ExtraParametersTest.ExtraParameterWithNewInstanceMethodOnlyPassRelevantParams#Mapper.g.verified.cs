﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::B Map(global::A src, int value)
    {
        var target = new global::B()
        {
            Value = value.ToString()
        };
        target.StringValue = src.StringValue;
        target.Nest = MapToD(src.Nest);
        return target;
    }

    private global::D MapToD(global::C source)
    {
        var target = new global::D(source.V);
        return target;
    }
}
