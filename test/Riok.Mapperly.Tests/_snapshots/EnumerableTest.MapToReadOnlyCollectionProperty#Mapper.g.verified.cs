﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::B Map(global::A source)
    {
        var target = new global::B();
        foreach (var item in source.Value)
        {
            target.Value.Add((long)item);
        }
        return target;
    }
}