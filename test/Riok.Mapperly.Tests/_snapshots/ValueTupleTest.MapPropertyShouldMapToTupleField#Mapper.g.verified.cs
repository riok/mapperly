﻿//HintName: Mapper.g.cs
// <auto-generated />
#pragma warning disable CS0618
#nullable enable
public partial class Mapper
{
    private partial (global::A, int) Map((global::B, string) source)
    {
        var target = (MapToA(source.Item1), int.Parse(source.Item2));
        target.Item1.Value = int.Parse(source.Item2);
        return target;
    }

    private global::A MapToA(global::B source)
    {
        var target = new global::A();
        target.Value = source.Value;
        return target;
    }
}