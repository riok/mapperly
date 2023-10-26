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
    private const string AccessorClassName = "UnsafeAccessor";

    public static MemberDeclarationSyntax BuildUnsafeAccessorClass(
        MapperDescriptor descriptor,
        CancellationToken cancellationToken,
        SourceEmitterContext ctx
    )
    {
        var accessorCtx = ctx.AddIndentation();
        var accessorClassName = descriptor.NameBuilder.New(AccessorClassName);
        var accessors = BuildUnsafeAccessors(accessorCtx, descriptor, cancellationToken);
        accessors = accessors.SeparateByLineFeed(accessorCtx.SyntaxFactory.Indentation);
        return ctx.SyntaxFactory.Class(
            accessorClassName,
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
            yield return accessor.BuildMethod(ctx);
        }
    }
}
#endif
