using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Riok.Mapperly.Output;
using Riok.Mapperly.Templates;

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

    public static void EmitTemplates(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<TemplateContent> templates
    )
    {
        context.RegisterImplementationSourceOutput(
            templates,
            static (spc, template) => spc.AddSource(template.FileName, SourceText.From(template.Content, Encoding.UTF8))
        );
    }

#if !ROSLYN4_4_OR_GREATER
    public static IncrementalValuesProvider<TSource> WhereNotNull<TSource>(this IncrementalValuesProvider<TSource?> source)
    {
#nullable disable
        return source.Where(x => x != null);
#nullable enable
    }

    public static IncrementalValueProvider<TSource> WithTrackingName<TSource>(this IncrementalValueProvider<TSource> source, string name) =>
        source;

    public static IncrementalValuesProvider<TSource> WithTrackingName<TSource>(
        this IncrementalValuesProvider<TSource> source,
        string name
    ) => source;
#endif
}
