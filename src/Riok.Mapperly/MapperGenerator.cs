using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly;

[Generator]
public partial class MapperGenerator
{
    private const string GeneratedFileSuffix = ".g.cs";
    public const string AddMappersStep = "ImplementationSourceOutput";
    public const string ReportDiagnosticsStep = "Diagnostics";

    public static readonly string MapperAttributeName = typeof(MapperAttribute).FullName;

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
        var wellKnownTypes = new WellKnownTypes(compilation);
        var mapperAttributeSymbol = wellKnownTypes.TryGet(MapperAttributeName);
        if (mapperAttributeSymbol == null)
            return MapperResults.Empty;

        var uniqueNameBuilder = new UniqueNameBuilder();

        var diagnostics = new List<Diagnostic>();
        var members = new List<MapperNode>();

        foreach (var mapperSyntax in mappers.Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mapperModel = compilation.GetSemanticModel(mapperSyntax.SyntaxTree);
            if (mapperModel.GetDeclaredSymbol(mapperSyntax) is not INamedTypeSymbol mapperSymbol)
                continue;

            var symbolAccessor = new SymbolAccessor(wellKnownTypes, compilation, mapperSymbol);
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

    private static ImmutableEquatableArray<Diagnostic> BuildCompilationDiagnostics(Compilation compilation)
    {
        if (compilation is CSharpCompilation { LanguageVersion: < LanguageVersion.CSharp9 } cSharpCompilation)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.LanguageVersionNotSupported,
                null,
                cSharpCompilation.LanguageVersion.ToDisplayString(),
                LanguageVersion.CSharp9.ToDisplayString()
            );
            return ImmutableEquatableArray.Create(diagnostic);
        }

        return ImmutableEquatableArray.Empty<Diagnostic>();
    }
}
