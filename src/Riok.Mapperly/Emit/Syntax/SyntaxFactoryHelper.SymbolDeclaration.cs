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
        var isPartial = modifiers.Any(kind => kind.IsKind(SyntaxKind.PartialKeyword));
        return ClassDeclaration(Identifier(name))
            .WithModifiers(modifiers)
            .WithMembers(members)
            .WithoutTrivia()
            .WithKeyword(TrailingSpacedToken(SyntaxKind.ClassKeyword))
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken))
            .WithAttributeLists(isPartial ? new SyntaxList<AttributeListSyntax>() : GeneratedCodeAttributeList())
            .AddLeadingLineFeed(Indentation);
    }
}
