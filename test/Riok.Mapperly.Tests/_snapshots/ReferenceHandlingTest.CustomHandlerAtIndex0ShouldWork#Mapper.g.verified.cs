﻿//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler, global::A a)
    {
        if (refHandler.TryGetReference<A, B>(a, out var existingTargetReference))
            return existingTargetReference;
        var target = new B();
        refHandler.SetReference<A, B>(a, target);
        target.Parent = Map(refHandler, a.Parent);
        target.Value = MapToD(a.Value, refHandler);
        return target;
    }

    private D MapToD(global::C source, global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<C, D>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new D();
        refHandler.SetReference<C, D>(source, target);
        target.StringValue = source.StringValue;
        return target;
    }
}
