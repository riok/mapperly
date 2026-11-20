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
            .WithAttributeLists(isPartial ? [] : [GeneratedCodeAttribute()])
            .AddLeadingLineFeed(Indentation);
    }

    public ClassDeclarationSyntax AddTypeParameters(ClassDeclarationSyntax syntax, INamedTypeSymbol type)
    {
        if (type.TypeParameters.Length == 0)
            return syntax;

        var typeParams = TypeParameterList(SeparatedList(type.TypeParameters.Select(tp => TypeParameter(Identifier(tp.Name)))));
        var indentation = Indentation + 1;
        var constraintClauses = TypeParameterConstraintClauses(type.TypeParameters).Select(c => c.AddLeadingLineFeed(indentation));

        return syntax.WithTypeParameterList(typeParams).WithConstraintClauses(List(constraintClauses));
    }

    public static IEnumerable<TypeParameterConstraintClauseSyntax> TypeParameterConstraintClauses(
        IEnumerable<ITypeParameterSymbol> typeParameters
    )
    {
        foreach (var tp in typeParameters)
        {
            var constraints = new List<TypeParameterConstraintSyntax>();

            if (tp.HasUnmanagedTypeConstraint)
            {
                constraints.Add(TypeConstraint(IdentifierName("unmanaged")).AddLeadingSpace());
            }
            else if (tp.HasValueTypeConstraint)
            {
                constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint).AddLeadingSpace());
            }
            else if (tp.HasNotNullConstraint)
            {
                constraints.Add(TypeConstraint(IdentifierName("notnull")).AddLeadingSpace());
            }
            else if (tp.HasReferenceTypeConstraint)
            {
                constraints.Add(
                    ClassOrStructConstraint(SyntaxKind.ClassConstraint)
                        .WithQuestionToken(
                            tp.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                                ? Token(SyntaxKind.QuestionToken)
                                : default
                        )
                        .AddLeadingSpace()
                );
            }

            foreach (var c in tp.ConstraintTypes)
            {
                constraints.Add(TypeConstraint(FullyQualifiedIdentifier(c)).AddLeadingSpace());
            }

            if (tp.HasConstructorConstraint)
            {
                constraints.Add(ConstructorConstraint().AddLeadingSpace());
            }

            if (constraints.Count == 0)
            {
                continue;
            }

            yield return TypeParameterConstraintClause(
                IdentifierName(tp.Name).AddLeadingSpace().AddTrailingSpace(),
                SeparatedList(constraints)
            );
        }
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
