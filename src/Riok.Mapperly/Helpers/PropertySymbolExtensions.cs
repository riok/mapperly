using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

public static class PropertySymbolExtensions
{
    public static bool IsInitOnly(this IPropertySymbol prop)
        => prop.SetMethod?.IsInitOnly == true;
}
