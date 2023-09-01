using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static MemberAccessExpressionSyntax MemberAccess(string identifierName, string propertyIdentifierName) =>
        MemberAccess(IdentifierName(identifierName), propertyIdentifierName);

    public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax idExpression, string propertyIdentifierName) =>
        MemberAccess(idExpression, IdentifierName(propertyIdentifierName));

    private static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax idExpression, SimpleNameSyntax property) =>
        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, idExpression, property);

    public static ElementAccessExpressionSyntax ElementAccess(ExpressionSyntax idExpression, ExpressionSyntax index) =>
        ElementAccessExpression(idExpression).WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(index))));

    public static ConditionalAccessExpressionSyntax ConditionalAccess(ExpressionSyntax idExpression, string propertyIdentifierName) =>
        ConditionalAccessExpression(idExpression, MemberBindingExpression(IdentifierName(propertyIdentifierName)));
}
