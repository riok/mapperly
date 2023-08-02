using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static class FieldSymbolEqualityComparer
{
    public static IEqualityComparer<IFieldSymbol?> Default = SymbolEqualityComparer.Default;
}
