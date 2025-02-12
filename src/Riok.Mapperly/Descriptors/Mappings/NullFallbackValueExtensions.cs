using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.Mappings;

internal static class NullFallbackValueExtensions
{
    public static bool IsNullable(this NullFallbackValue fallbackValue, ITypeSymbol targetType) =>
        fallbackValue == NullFallbackValue.Default && targetType.IsNullable();
}
