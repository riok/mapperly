using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class SourceEmitter
{
    public static CompilationUnitSyntax Build(MapperDescriptor descriptor)
    {
        var classDeclaration = ClassDeclaration(descriptor.Syntax.Identifier)
            .WithModifiers(descriptor.Syntax.Modifiers)
            .WithMembers(List(BuildMembers(descriptor)));

        return CompilationUnit()
            .WithMembers(SingletonList(WrapInNamespaceIfNeeded(descriptor.Namespace, classDeclaration)))
            .WithLeadingTrivia(Nullable(true))
            .NormalizeWhitespace();
    }

    private static IEnumerable<MemberDeclarationSyntax> BuildMembers(MapperDescriptor descriptor)
    {
        return descriptor.MethodTypeMappings.Select(mapping => mapping.BuildMethod());
    }

    private static MemberDeclarationSyntax WrapInNamespaceIfNeeded(string? namespaceName, MemberDeclarationSyntax classDeclaration)
    {
        return namespaceName == null
            ? classDeclaration
            : Namespace(namespaceName).WithMembers(SingletonList(classDeclaration));
    }
}
