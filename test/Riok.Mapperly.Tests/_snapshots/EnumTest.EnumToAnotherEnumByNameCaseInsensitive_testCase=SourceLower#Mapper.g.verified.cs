﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::B Map(global::A source)
    {
        var target = new global::B();
        target.Value = MapToD(source.Value);
        return target;
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private global::D MapToD(global::C source)
    {
        return source switch
        {
            global::C.value1 => global::D.Value1,
            _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum C is not supported"),
        };
    }
}