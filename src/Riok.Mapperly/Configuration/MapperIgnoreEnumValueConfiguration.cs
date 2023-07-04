using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

public record MapperIgnoreEnumValueConfiguration(IFieldSymbol Value);
