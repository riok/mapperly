﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::System.Collections.Generic.List<int> Map(global::System.ReadOnlyMemory<int> source)
    {
        return MapToListOfInt32(source.Span);
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private global::System.Collections.Generic.List<int> MapToListOfInt32(global::System.ReadOnlySpan<int> source)
    {
        var target = new global::System.Collections.Generic.List<int>(source.Length);
        foreach (var item in source)
        {
            target.Add(item);
        }
        return target;
    }
}