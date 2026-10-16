using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

public record UseStaticMapperConfiguration(INamedTypeSymbol MapperType)
{
    public required bool NoExpressionInlining { get; init; }
}
