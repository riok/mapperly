//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    public partial object Map(object source, global::System.Type destinationType)
    {
        var refHandler = new global::Riok.Mapperly.Abstractions.ReferenceHandling.Internal.PreserveReferenceHandler();
        return source switch
        {
            global::A x when destinationType.IsAssignableFrom(typeof(global::B)) => MapToB(x, refHandler),
            null => throw new System.ArgumentNullException(nameof(source)),
            _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {destinationType} as there is no known type mapping", nameof(source)),
        };
    }

    private partial global::B MapToB(global::A source, global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<global::A, global::B>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new global::B();
        refHandler.SetReference<global::A, global::B>(source, target);
        return target;
    }
}
