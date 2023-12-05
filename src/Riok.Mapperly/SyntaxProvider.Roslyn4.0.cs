#if !ROSLYN4_4_OR_GREATER

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly;

internal static class SyntaxProvider
{
    private static readonly SymbolDisplayFormat _fullyQualifiedFormatWithoutGlobal = SymbolDisplayFormat
        .FullyQualifiedFormat
        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining);

    public static IncrementalValuesProvider<MapperDeclaration> GetMapperDeclarations(IncrementalGeneratorInitializationContext context)
    {
        return context
            .SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                static (ctx, ct) => GetMapperDeclaration(ctx, ct)
            )
            .WhereNotNull();
    }

    public static IncrementalValueProvider<IAssemblySymbol?> GetMapperDefaultDeclarations(IncrementalGeneratorInitializationContext context)
    {
        return context
            .SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => s is CompilationUnitSyntax { AttributeLists.Count: > 0 },
                static (ctx, ct) => GetMapperDefaultDeclarations(ctx)
            )
            .Collect()
            .Select((x, _) => x.FirstOrDefault());
    }

    private static MapperDeclaration? GetMapperDeclaration(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        var declaration = (ClassDeclarationSyntax)ctx.Node;
        if (ctx.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol symbol)
            return null;

        return HasAttribute(symbol, MapperGenerator.MapperAttributeName) ? new MapperDeclaration(symbol, declaration) : null;
    }

    private static IAssemblySymbol? GetMapperDefaultDeclarations(GeneratorSyntaxContext ctx)
    {
        var symbol = ctx.SemanticModel.Compilation.Assembly;
        return HasAttribute(symbol, MapperGenerator.MapperDefaultsAttributeName) ? symbol : null;
    }

    private static bool HasAttribute(ISymbol symbol, string attributeName)
    {
        return symbol
            .GetAttributes()
            .Any(
                x =>
                    string.Equals(
                        x.AttributeClass?.ToDisplayString(_fullyQualifiedFormatWithoutGlobal),
                        attributeName,
                        StringComparison.Ordinal
                    )
            );
    }
}
#endif
