namespace Riok.Mapperly.Symbols;

/// <summary>
/// <see cref="UserDefinedNewInstanceRuntimeTargetTypeMapping"/> method parameters.
/// </summary>
public record RuntimeTargetTypeMappingMethodParameters(
    MethodParameter Source,
    MethodParameter TargetType,
    MethodParameter? ReferenceHandler
) : MappingMethodParameters(Source, null, ReferenceHandler);
