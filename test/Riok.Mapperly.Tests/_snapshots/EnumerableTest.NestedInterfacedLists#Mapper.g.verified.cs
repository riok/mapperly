﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::System.Collections.Generic.IReadOnlyList<global::System.Collections.Generic.IReadOnlyCollection<global::System.Collections.Generic.IReadOnlyList<string>>> Map(global::System.Collections.Generic.IReadOnlyList<global::System.Collections.Generic.IReadOnlyCollection<global::System.Collections.Generic.IReadOnlyList<int>>> source)
    {
        var target = new string[source.Count][][];
        var i = 0;
        foreach (var item in source)
        {
            target[i] = MapToStringArrayArray(item);
            i++;
        }
        return target;
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private string[] MapToStringArray(global::System.Collections.Generic.IReadOnlyCollection<int> source)
    {
        var target = new string[source.Count];
        var i = 0;
        foreach (var item in source)
        {
            target[i] = item.ToString();
            i++;
        }
        return target;
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private string[][] MapToStringArrayArray(global::System.Collections.Generic.IReadOnlyCollection<global::System.Collections.Generic.IReadOnlyList<int>> source)
    {
        var target = new string[source.Count][];
        var i = 0;
        foreach (var item in source)
        {
            target[i] = MapToStringArray(item);
            i++;
        }
        return target;
    }
}