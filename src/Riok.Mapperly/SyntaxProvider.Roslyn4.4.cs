#if ROSLYN4_4_OR_GREATER

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly;

internal static class SyntaxProvider
{
    public static IncrementalValueProvider<ImmutableArray<Compilation>> GetNestedCompilations(
        IncrementalGeneratorInitializationContext context
    ) =>
        context
            .MetadataReferencesProvider.Select((metadataReference, _) => (metadataReference as CompilationReference)?.Compilation!)
            .Where(x => x is not null)
            .Collect();

    public static IncrementalValuesProvider<MapperDeclaration> GetMapperDeclarations(IncrementalGeneratorInitializationContext context)
    {
        return context
            .SyntaxProvider.ForAttributeWithMetadataName(
                MapperGenerator.MapperAttributeName,
                static (s, _) => s is ClassDeclarationSyntax,
                static (ctx, _) => (ctx.TargetSymbol, TargetNode: (ClassDeclarationSyntax)ctx.TargetNode)
            )
            .Where(x => x.TargetSymbol is INamedTypeSymbol)
            .Select((x, _) => new MapperDeclaration((INamedTypeSymbol)x.TargetSymbol, x.TargetNode));
    }

    public static IncrementalValueProvider<IAssemblySymbol?> GetMapperDefaultDeclarations(IncrementalGeneratorInitializationContext context)
    {
        return context
            .SyntaxProvider.ForAttributeWithMetadataName(
                MapperGenerator.MapperDefaultsAttributeName,
                static (s, _) => s is CompilationUnitSyntax,
                static (ctx, _) => (IAssemblySymbol)ctx.TargetSymbol
            )
            .Collect()
            .Select((x, _) => x.FirstOrDefault());
    }
}
#endif
