namespace Riok.Mapperly.Symbols;

/// <summary>
/// Well-known mapping method parameters.
/// </summary>
/// <param name="ResultOut">
/// The out parameter to assign the value, if any. This value will be null if the method does not have
/// an out parameter or if the method does not return a bool.
/// </param>
public record MappingMethodParameters(
    MethodParameter Source,
    MethodParameter? Target,
    MethodParameter? ReferenceHandler,
    IReadOnlyCollection<MethodParameter> AdditionalParameters,
    MethodParameter? ResultOut
);
