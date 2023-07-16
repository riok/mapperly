using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Symbols;

public record RuntimeTargetTypeMapping(INewInstanceMapping Mapping, bool IsAssignableToMethodTargetType);
