using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly;

internal static class SyntaxProvider
{
    public static IncrementalValueProvider<ImmutableArray<Compilation>> GetNestedCompilations(
        IncrementalGeneratorInitializationContext context
    )
    {
        return context
            .MetadataReferencesProvider.OfType<MetadataReference, CompilationReference>()
            .Select((x, _) => x.Compilation)
            .Collect();
    }

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

    public static IncrementalValueProvider<ImmutableArray<AttributeData>> GetUseStaticMapperDeclarations(
        IncrementalGeneratorInitializationContext context
    )
    {
        var staticMapperAttributes = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                MapperGenerator.UseStaticMapperName,
                static (s, _) => s is CompilationUnitSyntax,
                static (ctx, _) => ctx.Attributes
            )
            .SelectMany(static (x, _) => x)
            .Collect();
        var genericStaticMapperAttributes = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                MapperGenerator.UseStaticMapperGenericName,
                static (s, _) => s is CompilationUnitSyntax,
                static (ctx, _) => ctx.Attributes
            )
            .SelectMany(static (x, _) => x)
            .Collect();

        return staticMapperAttributes.Combine(genericStaticMapperAttributes).SelectMany((x, _) => x.Left.AddRange(x.Right)).Collect();
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
