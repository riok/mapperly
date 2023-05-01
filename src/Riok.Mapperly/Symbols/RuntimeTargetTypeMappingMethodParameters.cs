using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// <see cref="UserDefinedNewInstanceRuntimeTargetTypeMapping"/> method parameters.
/// </summary>
public record struct RuntimeTargetTypeMappingMethodParameters(
    MethodParameter Source,
    MethodParameter TargetType,
    MethodParameter? ReferenceHandler
);
