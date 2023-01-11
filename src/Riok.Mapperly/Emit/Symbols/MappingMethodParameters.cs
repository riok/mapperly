namespace Riok.Mapperly.Emit.Symbols;

/// <summary>
/// Well known mapping method parameters.
/// </summary>
public readonly struct MappingMethodParameters
{
    public MappingMethodParameters(MethodParameter source, MethodParameter? target, MethodParameter? referenceHandler)
    {
        Source = source;
        Target = target;
        ReferenceHandler = referenceHandler;
    }

    public MethodParameter Source { get; }

    public MethodParameter? Target { get; }

    public MethodParameter? ReferenceHandler { get; }
}
