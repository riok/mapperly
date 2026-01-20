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

    // UTF-8 encoding without BOM (default for 'charset = utf-8' in .editorconfig)
    private static readonly Encoding _utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    // UTF-8 encoding with BOM (for 'charset = utf-8-bom' in .editorconfig)
    private static readonly Encoding _utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

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
                // Respect .editorconfig charset setting
                // Default to UTF-8 without BOM (most common for source files)
                var encoding = mapper.Charset switch
                {
                    "utf-8-bom" => _utf8WithBom,
                    "utf-16be" => Encoding.BigEndianUnicode,
                    "utf-16le" => Encoding.Unicode,
                    _ => _utf8NoBom,
                };

                // Respect .editorconfig end_of_line setting
                // The syntax tree uses CRLF by default (ElasticCarriageReturnLineFeed)
                // For non-CRLF, use streaming replacement to avoid intermediate string allocation
                var text = GetSourceText(mapper.Body, mapper.EndOfLine);

                // Always use SourceText.From to ensure BOM is included when specified
                spc.AddSource(mapper.FileName, SourceText.From(text, encoding));
            }
        );
    }

    private static string GetSourceText(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax body, string? endOfLine)
    {
        var newLine = endOfLine switch
        {
            "lf" => "\n",
            "cr" => "\r",
            _ => null, // crlf, null, empty, or unknown - no replacement needed
        };

        if (newLine == null)
        {
            return body.GetText().ToString();
        }

        // Streaming replacement: write to StringBuilder via custom TextWriter
        // This avoids creating an intermediate CRLF string
        var sb = new StringBuilder();
        using (var writer = new LineEndingTextWriter(sb, newLine))
        {
            body.WriteTo(writer);
        }

        return sb.ToString();
    }
}
