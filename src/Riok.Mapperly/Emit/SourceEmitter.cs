using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class SourceEmitter
{
    public static string Build(MapperDescriptor descriptor)
    {
        var classDeclaration = ClassDeclaration(descriptor.Name)
            .WithModifiers(TokenList(Accessibility(descriptor.Accessibility), Token(SyntaxKind.SealedKeyword)))
            .WithBaseList(BaseList(descriptor.BaseName))
            .WithMembers(List(BuildMembers(descriptor)));
        var compilationUnit = CompilationUnit()
            .WithMembers(SingletonList(WrapInNamespaceIfNeeded(descriptor.Namespace, classDeclaration)))
            .WithLeadingTrivia(Nullable(true))
            .NormalizeWhitespace();
        return compilationUnit.ToFullString();
    }

    private static IEnumerable<MemberDeclarationSyntax> BuildMembers(MapperDescriptor descriptor)
    {
        // public static readonly MyMapper Instance = new MyMapper();
        if (!string.IsNullOrEmpty(descriptor.InstanceName))
        {
            var ctor = ObjectCreationExpression(IdentifierName(descriptor.Name)).WithArgumentList(ArgumentList());
            yield return DeclareField(
                descriptor.BaseName,
                descriptor.InstanceName!,
                ctor,
                SyntaxKind.PublicKeyword,
                SyntaxKind.StaticKeyword,
                SyntaxKind.ReadOnlyKeyword);
        }

        // mapping methods
        foreach (var mapping in descriptor.MethodTypeMappings)
        {
            yield return mapping.BuildMethod();
        }
    }

    private static MemberDeclarationSyntax WrapInNamespaceIfNeeded(string? namespaceName, MemberDeclarationSyntax classDeclaration)
    {
        return namespaceName == null
            ? classDeclaration
            : Namespace(namespaceName).WithMembers(SingletonList(classDeclaration));
    }
}
