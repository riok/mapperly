using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(this IncrementalValuesProvider<TSource?> source)
    {
#nullable disable
        return source.Where(x => x != null);
#nullable enable
    }
}
