﻿//HintName: MyMapper.g.cs
// <auto-generated />
#nullable enable
public partial class MyMapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public partial global::B Map(global::A a)
    {
        var target = new global::B(MapToInnerB(a.Value));
        return target;
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private global::InnerB MapToInnerB(global::InnerA source)
    {
        var target = new global::InnerB(source.Value);
        return target;
    }
}