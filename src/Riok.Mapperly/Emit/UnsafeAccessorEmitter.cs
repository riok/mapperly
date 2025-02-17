#if ROSLYN4_7_OR_GREATER
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class UnsafeAccessorEmitter
{
    public static MemberDeclarationSyntax BuildUnsafeAccessorClass(
        SourceEmitterContext ctx,
        MapperDescriptor descriptor,
        CancellationToken cancellationToken
    )
    {
        var accessorCtx = ctx.AddIndentation();
        var accessors = BuildUnsafeAccessors(accessorCtx, descriptor, cancellationToken);
        accessors = accessors.SeparateByLineFeed(accessorCtx.SyntaxFactory.Indentation);
        return ctx.SyntaxFactory.Class(
            descriptor.UnsafeAccessorName,
            TokenList(TrailingSpacedToken(SyntaxKind.StaticKeyword), TrailingSpacedToken(SyntaxKind.FileKeyword)),
            List(accessors)
        );
    }

    private static IEnumerable<MemberDeclarationSyntax> BuildUnsafeAccessors(
        SourceEmitterContext ctx,
        MapperDescriptor descriptor,
        CancellationToken cancellationToken
    )
    {
        foreach (var accessor in descriptor.UnsafeAccessors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return accessor.BuildAccessorMethod(ctx);
        }
    }
}
#endif
