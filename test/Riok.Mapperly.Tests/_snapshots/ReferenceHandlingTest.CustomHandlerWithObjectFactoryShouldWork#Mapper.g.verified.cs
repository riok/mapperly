//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B Map(A a, Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<A, B>(a, out var existingTargetReference))
            return existingTargetReference;
        var target = CreateB();
        refHandler.SetReference<A, B>(a, target);
        target.Parent = Map(a.Parent, refHandler);
        target.Value = MapToD(a.Value, refHandler);
        return target;
    }

    private D MapToD(C source, Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<C, D>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new D();
        refHandler.SetReference<C, D>(source, target);
        target.StringValue = source.StringValue;
        return target;
    }
}