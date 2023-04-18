﻿//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial global::B Map(global::A source)
    {
        var target = new global::B();
        if (source.Nested != null)
        {
            if (source.Nested?.Value2 != null)
            {
                target.NestedValue2 = source.Nested.Value2.Value;
            }

            target.Nested = MapToD(source.Nested);
        }

        return target;
    }

    private global::D MapToD(global::C source)
    {
        var target = new global::D();
        if (source.Value1 != null)
        {
            target.Value1 = source.Value1.Value;
        }

        return target;
    }
}