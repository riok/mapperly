﻿//HintName: Mapper.g.cs
// <auto-generated />
#pragma warning disable CS0618
#nullable enable
public partial class Mapper
{
    private partial (string E, (long G, int H) F) Map(((int B, int C) A, string D) source)
    {
        var target = (E: source.D, F: MapToValueTuple(source.A));
        target.F.H = int.Parse(source.D);
        return target;
    }

    private (long G, int H) MapToValueTuple((int B, int C) source)
    {
        var target = (G: (long)source.B, H: source.C);
        return target;
    }
}