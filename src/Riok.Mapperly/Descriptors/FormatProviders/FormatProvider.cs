using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.FormatProviders;

public record FormatProvider(string Name, bool Default, ISymbol Symbol);
