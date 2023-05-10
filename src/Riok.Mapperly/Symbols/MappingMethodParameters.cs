namespace Riok.Mapperly.Symbols;

/// <summary>
/// Well known mapping method parameters.
/// </summary>
public record struct MappingMethodParameters(MethodParameter Source, MethodParameter? Target, MethodParameter? ReferenceHandler);
