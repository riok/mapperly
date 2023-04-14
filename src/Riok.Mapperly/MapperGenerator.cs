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

    public static readonly string MapperAttributeName = typeof(MapperAttribute).FullName;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperClassDeclarations = SyntaxProvider.GetClassDeclarations(context);

        var compilationAndMappers = context.CompilationProvider.Combine(mapperClassDeclarations.Collect());
        context.RegisterImplementationSourceOutput(compilationAndMappers, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> mappers, SourceProductionContext ctx)
    {
        if (mappers.IsDefaultOrEmpty)
            return;

#if DEBUG_SOURCE_GENERATOR
        DebuggerUtil.AttachDebugger();
#endif
        var mapperAttributeSymbol = compilation.GetTypeByMetadataName(MapperAttributeName);
        if (mapperAttributeSymbol == null)
            return;

        var uniqueNameBuilder = new UniqueNameBuilder();
        foreach (var mapperSyntax in mappers.Distinct())
        {
            var mapperModel = compilation.GetSemanticModel(mapperSyntax.SyntaxTree);
            if (mapperModel.GetDeclaredSymbol(mapperSyntax) is not INamedTypeSymbol mapperSymbol)
                continue;

            if (!mapperSymbol.HasAttribute(mapperAttributeSymbol))
                continue;

            var builder = new DescriptorBuilder(ctx, compilation, mapperSyntax, mapperSymbol);
            var descriptor = builder.Build();

            ctx.AddSource(
                uniqueNameBuilder.New(mapperSymbol.Name) + GeneratedFileSuffix,
                SourceText.From(SourceEmitter.Build(descriptor).ToFullString(), Encoding.UTF8));
        }
    }
}
