using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record EnumMappingConfiguration(
    EnumMappingStrategy Strategy,
    bool IgnoreCase,
    IFieldSymbol? FallbackValue,
    IReadOnlyCollection<EnumValueMappingConfiguration> ExplicitMappings
);
