using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static MethodDeclarationSyntax PublicStaticExternMethod(
        TypeSyntax returnType,
        string methodName,
        ParameterListSyntax parameterList,
        SyntaxList<AttributeListSyntax> attributeList
    )
    {
        return MethodDeclaration(returnType, Identifier(methodName))
            .WithModifiers(
                TokenList(
                    TrailingSpacedToken(SyntaxKind.PublicKeyword),
                    TrailingSpacedToken(SyntaxKind.StaticKeyword),
                    TrailingSpacedToken(SyntaxKind.ExternKeyword)
                )
            )
            .WithParameterList(parameterList)
            .WithAttributeLists(attributeList)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}
