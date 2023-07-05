#if !ROSLYN4_0_OR_GREATER
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Riok.Mapperly;

public partial class MapperGenerator : ISourceGenerator
{
    public ISourceGenerator AsSourceGenerator() => this;

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxProvider());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxProvider = (SyntaxProvider)context.SyntaxContextReceiver!;
        var buildCompilationDiagnostics = BuildCompilationDiagnostics(context.Compilation);
        ReportDiagnostics(context, buildCompilationDiagnostics);

        var results = BuildDescriptors(context.Compilation, syntaxProvider.ClassDeclarations.ToImmutableArray(), default);
        ReportDiagnostics(context, results.Diagnostics);

        foreach (var source in results.Mappers)
        {
            var mapperText = source.Body.NormalizeWhitespace().ToFullString();
            context.AddSource(source.FileName, SourceText.From(mapperText, Encoding.UTF8));
        }
    }

    private void ReportDiagnostics(GeneratorExecutionContext context, IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }
}
#endif
