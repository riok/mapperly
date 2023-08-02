﻿//HintName: Mapper.g.cs
// <auto-generated />
#pragma warning disable CS0618
#nullable enable
public partial class Mapper
{
    private partial global::System.Collections.Generic.Stack<int> Map(global::System.ReadOnlyMemory<int> source)
    {
        return MapToStack(source.Span);
    }

    private global::System.Collections.Generic.Stack<int> MapToStack(global::System.ReadOnlySpan<int> source)
    {
        var target = new global::System.Collections.Generic.Stack<int>();
        target.EnsureCapacity(source.Length + target.Count);
        foreach (var item in source)
        {
            target.Push(item);
        }

        return target;
    }
}