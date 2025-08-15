using Riok.Mapperly.Descriptors.Mappings.UserMappings;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// <see cref="UserDefinedNewInstanceRuntimeTargetTypeMapping"/> method parameters.
/// </summary>
/// <inheritdoc cref="MappingMethodParameters"/>
public record RuntimeTargetTypeMappingMethodParameters(
    MethodParameter Source,
    MethodParameter TargetType,
    MethodParameter? ReferenceHandler,
    MethodParameter? ResultOut
) : MappingMethodParameters(Source, null, ReferenceHandler, [], ResultOut);
