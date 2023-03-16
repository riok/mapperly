using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

public static class PropertySymbolExtensions
{
    public static bool CanSet(this IPropertySymbol prop)
        => !prop.IsReadOnly && prop.SetMethod?.IsAccessible() != false;

    public static bool CanGet(this IPropertySymbol prop)
        => !prop.IsWriteOnly && prop.GetMethod?.IsAccessible() != false;

    public static bool CanOnlySetViaInitializer(this IPropertySymbol prop)
        => prop.IsInitOnly() || prop.IsRequired();

    public static bool IsInitOnly(this IPropertySymbol prop)
        => prop.SetMethod?.IsInitOnly == true;

    public static bool IsRequired(this IPropertySymbol prop)
#if ROSLYN4_4_OR_GREATER
        => prop.IsRequired;
#else
        => false;
#endif
}
