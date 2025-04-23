using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
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
            .WithAttributeLists(isPartial ? [] : [GeneratedCodeAttribute()])
            .AddLeadingLineFeed(Indentation);
    }

    public ClassDeclarationSyntax AddTypeParameters(ClassDeclarationSyntax syntax, INamedTypeSymbol type)
    {
        if (type.TypeParameters.Length == 0)
            return syntax;

        var typeParams = TypeParameterList(SeparatedList(type.TypeParameters.Select(tp => TypeParameter(Identifier(tp.Name)))));
        syntax = syntax.WithTypeParameterList(typeParams);

        foreach (var typeParam in type.TypeParameters)
        {
            var constraints = new List<TypeParameterConstraintSyntax>();

            if (typeParam.HasNotNullConstraint)
                constraints.Add(TypeConstraint(IdentifierName("notnull")));

            if (typeParam.HasReferenceTypeConstraint)
                constraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint));

            if (typeParam.HasUnmanagedTypeConstraint)
                constraints.Add(TypeConstraint(IdentifierName("unmanaged")));

            if (typeParam.HasValueTypeConstraint)
                constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint));

            foreach (var constraintType in typeParam.ConstraintTypes)
            {
                var typeConstraint = TypeConstraint(ParseTypeName(constraintType.FullyQualifiedIdentifierName()));
                constraints.Add(typeConstraint);
            }

            if (typeParam.HasConstructorConstraint)
                constraints.Add(ConstructorConstraint());

            if (constraints.Count == 0)
                continue;

            var constraintClause = TypeParameterConstraintClause(
                IdentifierName(typeParam.Name).AddLeadingSpace().AddTrailingSpace(),
                SeparatedList(constraints.Select(c => c.AddLeadingSpace()))
            );

            syntax = syntax.AddConstraintClauses(constraintClause.AddLeadingLineFeed(Indentation + 1));
        }

        return syntax;
    }

    public TypeDeclarationSyntax TypeDeclaration(TypeDeclarationSyntax syntax, SyntaxList<MemberDeclarationSyntax> members)
    {
        var name = Identifier(syntax.Identifier.ValueText);
        TypeDeclarationSyntax type = syntax switch
        {
            ClassDeclarationSyntax => ClassDeclaration(name),
            StructDeclarationSyntax => StructDeclaration(name),
            InterfaceDeclarationSyntax => InterfaceDeclaration(name),
            RecordDeclarationSyntax => RecordDeclaration(Token(SyntaxKind.RecordKeyword), name),
            _ => throw new NotSupportedException($"Unsupported type declaration syntax {syntax.GetType().Name}."),
        };

        var isPartial = syntax.Modifiers.Any(kind => kind.IsKind(SyntaxKind.PartialKeyword));
        return type.WithModifiers(syntax.Modifiers)
            .WithMembers(members)
            .WithoutTrivia()
            .WithKeyword(TrailingSpacedToken(syntax.Keyword.Kind()))
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken))
            .WithAttributeLists(isPartial ? [] : [GeneratedCodeAttribute()])
            .AddLeadingLineFeed(Indentation);
    }
}
