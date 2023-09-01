using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class SourceEmitter
{
    private const string AutoGeneratedComment = "// <auto-generated />";

    public static CompilationUnitSyntax Build(MapperDescriptor descriptor, CancellationToken cancellationToken)
    {
        var ctx = new SourceEmitterContext(
            descriptor.Symbol.IsStatic,
            descriptor.NameBuilder,
            new SyntaxFactoryHelper(descriptor.Symbol.ContainingAssembly.Name)
        );
        ctx = IndentForMapper(ctx, descriptor.Symbol);

        var memberCtx = ctx.AddIndentation();
        var members = BuildMembers(memberCtx, descriptor, cancellationToken);
        members = members.SeparateByLineFeed(memberCtx.SyntaxFactory.Indentation);
        MemberDeclarationSyntax member = ctx.SyntaxFactory.Class(descriptor.Symbol.Name, descriptor.Syntax.Modifiers, List(members));

        ctx = ctx.RemoveIndentation();
        member = WrapInClassesAsNeeded(ref ctx, descriptor.Symbol, member);
        member = WrapInNamespaceIfNeeded(ctx, descriptor.Namespace, member);

        return CompilationUnit()
            .WithMembers(SingletonList(member))
            .WithLeadingTrivia(Comment(AutoGeneratedComment), ElasticCarriageReturnLineFeed, Nullable(true), ElasticCarriageReturnLineFeed);
    }

    private static IEnumerable<MemberDeclarationSyntax> BuildMembers(
        SourceEmitterContext ctx,
        MapperDescriptor descriptor,
        CancellationToken cancellationToken
    )
    {
        foreach (var mapping in descriptor.MethodTypeMappings)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return mapping.BuildMethod(ctx);
        }
    }

    private static MemberDeclarationSyntax WrapInClassesAsNeeded(
        ref SourceEmitterContext ctx,
        INamedTypeSymbol symbol,
        MemberDeclarationSyntax syntax
    )
    {
        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            if (containingType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not ClassDeclarationSyntax containingTypeSyntax)
                break;

            syntax = ctx.SyntaxFactory.Class(containingType.Name, containingTypeSyntax.Modifiers, SingletonList(syntax));
            ctx = ctx.RemoveIndentation();
            containingType = containingType.ContainingType;
        }

        return syntax;
    }

    private static MemberDeclarationSyntax WrapInNamespaceIfNeeded(
        SourceEmitterContext ctx,
        string? namespaceName,
        MemberDeclarationSyntax classDeclaration
    )
    {
        if (namespaceName == null)
            return classDeclaration;

        return ctx.SyntaxFactory.Namespace(namespaceName).WithMembers(SingletonList(classDeclaration));
    }

    private static SourceEmitterContext IndentForMapper(SourceEmitterContext ctx, INamedTypeSymbol symbol)
    {
        while (symbol.ContainingType != null)
        {
            ctx = ctx.AddIndentation();
            symbol = symbol.ContainingType;
        }

        return symbol.ContainingNamespace.ContainingNamespace == null ? ctx : ctx.AddIndentation();
    }
}
