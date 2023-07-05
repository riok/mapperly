#if ROSLYN4_0_OR_GREATER
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly;

public partial class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperClassDeclarations = SyntaxProvider.GetClassDeclarations(context);

        context.ReportDiagnostics(context.CompilationProvider.Select(static (compilation, ct) => BuildCompilationDiagnostics(compilation)));

        var compilationAndMappers = context.CompilationProvider.Combine(mapperClassDeclarations.Collect());
        var mappersWithDiagnostics = compilationAndMappers.Select(
            static (x, cancellationToken) => BuildDescriptors(x.Left, x.Right, cancellationToken)
        );

        // output the diagnostics
        context.ReportDiagnostics(
            mappersWithDiagnostics.Select(static (source, _) => source.Diagnostics).WithTrackingName(ReportDiagnosticsStep)
        );

        // split into mapper name pairs
        var mappers = mappersWithDiagnostics.SelectMany(static (x, _) => x.Mappers);

        context.RegisterImplementationSourceOutput(
            mappers,
            static (spc, source) =>
            {
                var mapperText = source.Body.NormalizeWhitespace().ToFullString();
                spc.AddSource(source.FileName, SourceText.From(mapperText, Encoding.UTF8));
            }
        );
    }
}
#endif
