﻿//HintName: Mapper.g.cs
// <auto-generated />
#pragma warning disable CS0618
#nullable enable
public partial class Mapper
{
    private partial global::B Map(global::A source)
    {
        var target = new global::B(source.StringValue);
        target.IntValue = source.IntValue;
        return target;
    }
}