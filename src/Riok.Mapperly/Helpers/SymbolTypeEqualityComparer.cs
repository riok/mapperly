using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static class SymbolTypeEqualityComparer
{
    public static readonly IEqualityComparer<ITypeParameterSymbol?> TypeParameterDefault = SymbolEqualityComparer.Default;
    public static readonly IEqualityComparer<IFieldSymbol?> FieldDefault = SymbolEqualityComparer.Default;
    public static readonly IEqualityComparer<IMethodSymbol?> MethodDefault = SymbolEqualityComparer.Default;
}
