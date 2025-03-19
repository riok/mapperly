using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public MethodDeclarationSyntax PublicStaticExternMethod(
        TypeSyntax returnType,
        string methodName,
        ParameterListSyntax parameterList,
        SyntaxList<AttributeListSyntax> attributes
    )
    {
        attributes = attributes.Insert(0, GeneratedCodeAttribute());
        return MethodDeclaration(returnType, Identifier(methodName))
            .WithModifiers(
                TokenList(
                    TrailingSpacedToken(SyntaxKind.PublicKeyword),
                    TrailingSpacedToken(SyntaxKind.StaticKeyword),
                    TrailingSpacedToken(SyntaxKind.ExternKeyword)
                )
            )
            .WithParameterList(parameterList)
            .WithAttributeLists(attributes)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}
