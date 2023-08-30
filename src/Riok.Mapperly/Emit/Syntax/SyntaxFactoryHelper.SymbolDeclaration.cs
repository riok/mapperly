using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public NamespaceDeclarationSyntax Namespace(string ns)
    {
        return NamespaceDeclaration(IdentifierName(ns))
            .WithNamespaceKeyword(TrailingSpacedToken(SyntaxKind.NamespaceKeyword))
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken));
    }

    public ClassDeclarationSyntax Class(string name, SyntaxTokenList modifiers, SyntaxList<MemberDeclarationSyntax> members)
    {
        return ClassDeclaration(Identifier(name))
            .WithModifiers(modifiers)
            .WithMembers(members)
            .WithoutTrivia()
            .WithKeyword(TrailingSpacedToken(SyntaxKind.ClassKeyword))
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken))
            .AddLeadingLineFeed(Indentation);
    }
}
