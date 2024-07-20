using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static ArrayCreationExpressionSyntax CreateArray(ITypeSymbol type, ExpressionSyntax length)
    {
        var rankSpecifiers = new List<ArrayRankSpecifierSyntax> { ArrayRankSpecifier(SingletonSeparatedList(length)) };
        while (type.IsArrayType(out var nestedArrayType))
        {
            type = nestedArrayType.ElementType;
            rankSpecifiers.Add(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())));
        }

        var arrayType = ArrayType(FullyQualifiedIdentifier(type)).WithRankSpecifiers(List(rankSpecifiers));

        return ArrayCreationExpression(TrailingSpacedToken(SyntaxKind.NewKeyword), arrayType, default);
    }
}
