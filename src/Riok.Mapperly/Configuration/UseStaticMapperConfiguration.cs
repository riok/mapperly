using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

public record UseStaticMapperConfiguration(ITypeSymbol MapperType);
