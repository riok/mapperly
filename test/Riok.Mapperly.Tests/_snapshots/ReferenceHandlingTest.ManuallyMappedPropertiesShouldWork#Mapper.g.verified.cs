//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial B MapToB(A source)
    {
        return MapToB2(source, new Riok.Mapperly.Abstractions.ReferenceHandling.Internal.PreserveReferenceHandler());
    }

    private partial B MapToB1(A source)
    {
        return MapToB3(source, new Riok.Mapperly.Abstractions.ReferenceHandling.Internal.PreserveReferenceHandler());
    }

    private B MapToB2(A source, Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<A, B>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new B();
        refHandler.SetReference<A, B>(source, target);
        target.Parent = MapToB2(source.Parent, refHandler);
        target.MyValue = MapToD(source.Value, refHandler);
        return target;
    }

    private B MapToB3(A source, Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<A, B>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new B();
        refHandler.SetReference<A, B>(source, target);
        target.Parent = MapToB2(source.Parent, refHandler);
        target.MyValue2 = MapToD(source.Value, refHandler);
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