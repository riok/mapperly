using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public static InvocationExpressionSyntax GenericInvocation(
        string receiver,
        string methodName,
        IEnumerable<TypeSyntax> typeParams,
        params ExpressionSyntax[] arguments
    )
    {
        var method = GenericName(methodName).WithTypeArgumentList(TypeArgumentList(typeParams.ToArray()));
        return InvocationExpression(MemberAccess(IdentifierName(receiver), method)).WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax GenericInvocation(
        string methodName,
        IEnumerable<TypeSyntax> typeParams,
        params ExpressionSyntax[] arguments
    )
    {
        var method = GenericName(methodName).WithTypeArgumentList(TypeArgumentList(typeParams.ToArray()));
        return InvocationExpression(method).WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax Invocation(string methodName, params MethodArgument?[] arguments) =>
        Invocation(IdentifierName(methodName), arguments);

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method, params MethodArgument?[] arguments) =>
        Invocation(method, arguments.WhereNotNull().OrderBy(x => x.Parameter.Ordinal).Select(x => x.Argument).ToArray());

    public static InvocationExpressionSyntax Invocation(string methodName, params ExpressionSyntax[] arguments) =>
        Invocation(IdentifierName(methodName), arguments);

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method, params ExpressionSyntax[] arguments)
    {
        return InvocationExpression(method).WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method) => Invocation(method, Array.Empty<ArgumentSyntax>());

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method, params ArgumentSyntax[] arguments)
    {
        return InvocationExpression(method).WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ExpressionSyntax[] arguments)
    {
        var receiver = method.ReceiverType ?? throw new ArgumentException(nameof(method.ReceiverType) + " is null", nameof(method));
        var qualifiedReceiverName = receiver.NonNullable().FullyQualifiedIdentifierName();
        return StaticInvocation(qualifiedReceiverName, method.Name, arguments);
    }

    public static InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ArgumentSyntax[] arguments)
    {
        var receiver = method.ReceiverType ?? throw new ArgumentException(nameof(method.ReceiverType) + " is null", nameof(method));
        var qualifiedReceiverName = receiver.NonNullable().FullyQualifiedIdentifierName();

        var receiverTypeIdentifier = IdentifierName(qualifiedReceiverName);
        var methodAccess = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            receiverTypeIdentifier,
            IdentifierName(method.Name)
        );
        return InvocationExpression(methodAccess).WithArgumentList(ArgumentList(arguments));
    }

    public static TypeParameterListSyntax TypeParameterList(params ITypeParameterSymbol?[] parameters)
    {
        var typeParameters = parameters.WhereNotNull().OrderBy(x => x.Ordinal).Select(x => TypeParameter(x.Name));
        return SyntaxFactory.TypeParameterList(CommaSeparatedList(typeParameters));
    }

    public static ParameterListSyntax ParameterList(bool extensionMethod, params MethodParameter?[] parameters)
    {
        var parameterSyntaxes = parameters
            .WhereNotNull()
            .DistinctBy(x => x.Ordinal)
            .OrderBy(x => x.Ordinal)
            .Select(p => Parameter(extensionMethod, p));
        return SyntaxFactory.ParameterList(CommaSeparatedList(parameterSyntaxes));
    }

    private static ParameterSyntax Parameter(bool addThisKeyword, MethodParameter parameter)
    {
        return Parameter(parameter.Type.FullyQualifiedIdentifierName(), parameter.Name, addThisKeyword);
    }

    public static ParameterSyntax Parameter(string type, string identifier, bool addThisKeyword = false)
    {
        var param = SyntaxFactory.Parameter(Identifier(identifier)).WithType(IdentifierName(type).AddTrailingSpace());

        if (addThisKeyword)
        {
            param = param.WithModifiers(TokenList(TrailingSpacedToken(SyntaxKind.ThisKeyword)));
        }

        return param;
    }

    public static InvocationExpressionSyntax StaticInvocation(string receiverType, string methodName, params ExpressionSyntax[] arguments)
    {
        var receiverTypeIdentifier = IdentifierName(receiverType);
        var methodAccess = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            receiverTypeIdentifier,
            IdentifierName(methodName)
        );
        return InvocationExpression(methodAccess).WithArgumentList(ArgumentList(arguments));
    }

    public static string StaticMethodString(IMethodSymbol method)
    {
        var receiver = method.ReceiverType ?? throw new ArgumentException(nameof(method.ReceiverType) + " is null", nameof(method));
        var qualifiedReceiverName = receiver.NonNullable().FullyQualifiedIdentifierName();
        return $"{qualifiedReceiverName}.{method.Name}";
    }

    private static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argSyntaxes) =>
        SyntaxFactory.ArgumentList(CommaSeparatedList(argSyntaxes.Select(Argument)));

    public static TypeArgumentListSyntax TypeArgumentList(params TypeSyntax[] argSyntaxes) =>
        SyntaxFactory.TypeArgumentList(CommaSeparatedList(argSyntaxes));

    public static TypeArgumentListSyntax TypeArgumentList(IEnumerable<TypeSyntax> argSyntaxes) =>
        SyntaxFactory.TypeArgumentList(CommaSeparatedList(argSyntaxes));

    private static ArgumentListSyntax ArgumentList(params ArgumentSyntax[] args) => SyntaxFactory.ArgumentList(CommaSeparatedList(args));

    private static ArgumentListSyntax ArgumentList(IEnumerable<ArgumentSyntax> args) =>
        SyntaxFactory.ArgumentList(CommaSeparatedList(args));
}
