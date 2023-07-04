using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

public record struct MappingConfigurationReference(IMethodSymbol? Method, ITypeSymbol Source, ITypeSymbol Target);
