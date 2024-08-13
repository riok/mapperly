using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public ObjectCreationExpressionSyntax CreateInstance<T>()
    {
        var type = IdentifierName($"global::{typeof(T).FullName}");
        return CreateObject(type, SyntaxFactory.ArgumentList());
    }

    public ObjectCreationExpressionSyntax CreateGenericInstance(
        string typeName,
        TypeArgumentListSyntax typeArguments,
        IEnumerable<ArgumentSyntax> arguments
    )
    {
        var type = GenericName(typeName).WithTypeArgumentList(typeArguments);
        return CreateObject(type, ArgumentList(arguments));
    }

    public static ObjectCreationExpressionSyntax CreateInstance(ITypeSymbol typeSymbol)
    {
        var type = NonNullableIdentifier(typeSymbol);
        return CreateObject(type, SyntaxFactory.ArgumentList());
    }

    public ObjectCreationExpressionSyntax CreateInstance(ITypeSymbol typeSymbol, params ExpressionSyntax[] args)
    {
        var type = NonNullableIdentifier(typeSymbol);
        return CreateObject(type, ArgumentList(args));
    }

    public ObjectCreationExpressionSyntax CreateInstance(ITypeSymbol typeSymbol, IEnumerable<ArgumentSyntax> args)
    {
        var type = NonNullableIdentifier(typeSymbol);
        return CreateObject(type, ArgumentList(args));
    }

    public InitializerExpressionSyntax ObjectInitializer(params ExpressionSyntax[] expressions)
    {
        return InitializerExpression(
            SyntaxKind.ObjectInitializerExpression,
            LeadingLineFeedToken(SyntaxKind.OpenBraceToken).AddTrailingLineFeed(Indentation + 1),
            AddIndentation().CommaLineFeedSeparatedList(expressions),
            LeadingLineFeedToken(SyntaxKind.CloseBraceToken)
        );
    }

    private static ObjectCreationExpressionSyntax CreateObject(TypeSyntax type, ArgumentListSyntax argumentList) =>
        ObjectCreationExpression(TrailingSpacedToken(SyntaxKind.NewKeyword), type, argumentList, default);
}
