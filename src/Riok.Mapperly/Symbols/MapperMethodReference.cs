using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols;

public record MapperMethodReference(IMethodSymbol Method, INamedTypeSymbol TargetType, ISymbol? TargetMember);
