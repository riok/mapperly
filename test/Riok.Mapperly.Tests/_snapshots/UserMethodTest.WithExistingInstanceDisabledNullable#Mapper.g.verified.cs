﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial void Map(global::A? source, global::B? target)
    {
        if (source == null || target == null)
            return;
        target.StringValue = source.StringValue;
    }
}