#if ROSLYN4_4_OR_GREATER

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly;

internal static class SyntaxProvider
{
    public static IncrementalValuesProvider<ClassDeclarationSyntax> GetClassDeclarations(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MapperGenerator.MapperAttributeName,
                static (s, _) => s is ClassDeclarationSyntax,
                static (ctx, _) => ctx.TargetNode as ClassDeclarationSyntax
            )
            .WhereNotNull();
    }
}
#endif
