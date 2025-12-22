using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Riok.Mapperly.Output;

namespace Riok.Mapperly.Helpers;

internal static class IncrementalValuesProviderExtensions
{
    public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(this IncrementalValuesProvider<TSource?> source)
        where TSource : struct
    {
#nullable disable
        return source.Where(x => x.HasValue).Select((x, _) => x!.Value);
#nullable enable
    }

    private static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(this IncrementalValuesProvider<TSource?> source)
    {
#nullable disable
        return source.Where(x => x != null);
#nullable enable
    }

    public static IncrementalValuesProvider<TTarget> OfType<TSource, TTarget>(this IncrementalValuesProvider<TSource> source)
        where TTarget : class
    {
        return source.Select((x, _) => x as TTarget).WhereNotNull();
    }

    /// <summary>
    /// Registers an output node into an <see cref="IncrementalGeneratorInitializationContext"/> to output a diagnostic.
    /// </summary>
    /// <param name="context">The input <see cref="IncrementalGeneratorInitializationContext"/> instance.</param>
    /// <param name="diagnostic">The input <see cref="IncrementalValuesProvider{TValues}"/> sequence of diagnostics.</param>
    public static void ReportDiagnostics(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<Diagnostic> diagnostic
    )
    {
        context.RegisterSourceOutput(diagnostic, static (context, diagnostic) => context.ReportDiagnostic(diagnostic));
    }

    /// <summary>
    /// Registers an output node into an <see cref="IncrementalGeneratorInitializationContext"/> to output diagnostics.
    /// </summary>
    /// <param name="context">The input <see cref="IncrementalGeneratorInitializationContext"/> instance.</param>
    /// <param name="diagnostics">The input <see cref="IncrementalValuesProvider{TValues}"/> sequence of diagnostics.</param>
    public static void ReportDiagnostics(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<ImmutableEquatableArray<Diagnostic>> diagnostics
    )
    {
        context.RegisterSourceOutput(
            diagnostics,
            static (context, diagnostics) =>
            {
                foreach (var diagnostic in diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
        );
    }

    /// <summary>
    /// Registers an implementation source output for the provided mappers.
    /// </summary>
    /// <param name="context">The context, on which the output is registered.</param>
    /// <param name="mappers">The mappers.</param>
    public static void EmitMapperSource(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<MapperNode> mappers
    )
    {
        context.RegisterImplementationSourceOutput(
            mappers,
            static (spc, mapper) =>
            {
                var mapperText = mapper.Body.ToFullString();
                spc.AddSource(mapper.FileName, SourceText.From(mapperText, Encoding.UTF8));
            }
        );
    }
}
