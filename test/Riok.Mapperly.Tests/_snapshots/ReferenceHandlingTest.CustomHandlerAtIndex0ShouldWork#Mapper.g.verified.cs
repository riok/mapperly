//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial global::B Map(global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler, global::A a)
    {
        if (refHandler.TryGetReference<global::A, global::B>(a, out var existingTargetReference))
            return existingTargetReference;
        var target = new global::B();
        refHandler.SetReference<global::A, global::B>(a, target);
        target.Parent = Map(refHandler, a.Parent);
        target.Value = MapToD(a.Value, refHandler);
        return target;
    }

    private global::D MapToD(global::C source, global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<global::C, global::D>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new global::D();
        refHandler.SetReference<global::C, global::D>(source, target);
        target.StringValue = source.StringValue;
        return target;
    }
}
