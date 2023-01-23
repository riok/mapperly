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
    private static readonly string _mapperAttributeName = typeof(MapperAttribute).FullName;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperClassDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => IsSyntaxTargetForGeneration(s),
                static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .WhereNotNull();

        var compilationAndMappers = context.CompilationProvider.Combine(mapperClassDeclarations.Collect());
        context.RegisterImplementationSourceOutput(compilationAndMappers, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
    {
        var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
        foreach (var attributeListSyntax in classDeclaration.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();
                if (fullName == _mapperAttributeName)
                    return classDeclaration;
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> mappers, SourceProductionContext ctx)
    {
        if (mappers.IsDefaultOrEmpty)
            return;

#if DEBUG_SOURCE_GENERATOR
        DebuggerUtil.AttachDebugger();
#endif

        var mapperAttributeSymbol = compilation.GetTypeByMetadataName(_mapperAttributeName);
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
