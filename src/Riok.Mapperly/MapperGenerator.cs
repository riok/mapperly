using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly;

[Generator]
public class MapperGenerator : IIncrementalGenerator
{
    private const string GeneratedFileSuffix = ".g.cs";
    public const string AddMappersStep = "ImplementationSourceOutput";
    public const string ReportDiagnosticsStep = "Diagnostics";

    public static readonly string MapperAttributeName = typeof(MapperAttribute).FullName;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperClassDeclarations = SyntaxProvider.GetClassDeclarations(context);

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

    private static MapperResults BuildDescriptors(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> mappers,
        CancellationToken cancellationToken
    )
    {
        if (mappers.IsDefaultOrEmpty)
            return MapperResults.Empty;

#if DEBUG_SOURCE_GENERATOR
        DebuggerUtil.AttachDebugger();
#endif
        var mapperAttributeSymbol = compilation.GetTypeByMetadataName(MapperAttributeName);
        if (mapperAttributeSymbol == null)
            return MapperResults.Empty;

        var wellKnownTypes = new WellKnownTypes(compilation);
        var symbolAccessor = new SymbolAccessor(wellKnownTypes);
        var uniqueNameBuilder = new UniqueNameBuilder();

        var diagnostics = new List<Diagnostic>();
        var members = new List<MapperNode>();

        foreach (var mapperSyntax in mappers.Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mapperModel = compilation.GetSemanticModel(mapperSyntax.SyntaxTree);
            if (mapperModel.GetDeclaredSymbol(mapperSyntax) is not INamedTypeSymbol mapperSymbol)
                continue;

            if (!symbolAccessor.HasAttribute<MapperAttribute>(mapperSymbol))
                continue;

            var builder = new DescriptorBuilder(compilation, mapperSyntax, mapperSymbol, wellKnownTypes, symbolAccessor);
            var (descriptor, descriptorDiagnostics) = builder.Build();

            diagnostics.AddRange(descriptorDiagnostics);
            members.Add(new MapperNode(SourceEmitter.Build(descriptor), uniqueNameBuilder.New(descriptor.Name) + GeneratedFileSuffix));
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new MapperResults(members.ToImmutableEquatableArray(), diagnostics.ToImmutableEquatableArray());
    }
}
