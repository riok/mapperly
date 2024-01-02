#if !ROSLYN4_4_OR_GREATER
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Helpers;

internal static partial class IncrementalValuesProviderExtensions
{
    public static IncrementalValueProvider<TSource> WithTrackingName<TSource>(this IncrementalValueProvider<TSource> source, string name) =>
        source;

    public static IncrementalValuesProvider<TSource> WithTrackingName<TSource>(
        this IncrementalValuesProvider<TSource> source,
        string name
    ) => source;
}
#endif
