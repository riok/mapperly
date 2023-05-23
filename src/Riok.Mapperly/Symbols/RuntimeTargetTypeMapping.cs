using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Symbols;

public record RuntimeTargetTypeMapping(ITypeMapping Mapping, bool IsAssignableToMethodTargetType);
