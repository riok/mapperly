using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

public static class PropertySymbolExtensions
{
    public static bool IsInitOnly(this IPropertySymbol prop)
        => prop.SetMethod?.IsInitOnly == true;

    public static bool IsRequired(this IPropertySymbol prop)
#if ROSLYN4_4_OR_GREATER
        => prop.IsRequired;
#else
        => false;
#endif
}
