using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public InvocationExpressionSyntax GenericInvocation(
        string receiver,
        string methodName,
        IEnumerable<TypeSyntax> typeParams,
        params ExpressionSyntax[] arguments
    )
    {
        var method = GenericName(methodName).WithTypeArgumentList(TypeArgumentList(typeParams.ToArray()));
        return InvocationExpression(MemberAccess(IdentifierName(receiver), method)).WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax GenericInvocationWithoutIndention(
        string methodName,
        IEnumerable<TypeSyntax> typeParams,
        params ExpressionSyntax[] arguments
    )
    {
        var method = GenericName(methodName).WithTypeArgumentList(TypeArgumentList(typeParams.ToArray()));
        return InvocationExpression(method).WithArgumentList(ArgumentListWithoutIndention(arguments));
    }

    public InvocationExpressionSyntax Invocation(string methodName, params MethodArgument?[] arguments) =>
        Invocation(IdentifierName(methodName), arguments);

    public InvocationExpressionSyntax Invocation(ExpressionSyntax method, params MethodArgument?[] arguments) =>
        Invocation(method, arguments.WhereNotNull().OrderBy(x => x.Parameter.Ordinal).Select(x => x.Argument).ToArray());

    public InvocationExpressionSyntax Invocation(string methodName, params ExpressionSyntax[] arguments) =>
        Invocation(IdentifierName(methodName), arguments);

    public static InvocationExpressionSyntax InvocationWithoutIndention(string methodName, params ExpressionSyntax[] arguments) =>
        InvocationWithoutIndention(IdentifierName(methodName), arguments);

    public static InvocationExpressionSyntax InvocationWithoutIndention(ExpressionSyntax method, params ExpressionSyntax[] arguments) =>
        InvocationExpression(method).WithArgumentList(ArgumentListWithoutIndention(arguments));

    public InvocationExpressionSyntax Invocation(ExpressionSyntax method, params ExpressionSyntax[] arguments) =>
        InvocationExpression(method).WithArgumentList(ArgumentList(arguments));

    public InvocationExpressionSyntax Invocation(string methodName) => Invocation(IdentifierName(methodName));

    public InvocationExpressionSyntax Invocation(ExpressionSyntax method) => Invocation(method, Array.Empty<ArgumentSyntax>());

    public InvocationExpressionSyntax Invocation(ExpressionSyntax method, params ArgumentSyntax[] arguments)
    {
        return InvocationExpression(method).WithArgumentList(ArgumentList(arguments));
    }

    public InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ExpressionSyntax[] arguments)
    {
        var receiver = method.ReceiverType ?? throw new ArgumentException(nameof(method.ReceiverType) + " is null", nameof(method));
        var qualifiedReceiverName = receiver.NonNullable().FullyQualifiedIdentifierName();
        return StaticInvocation(qualifiedReceiverName, method.Name, arguments);
    }

    public InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ArgumentSyntax[] arguments)
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

    public static TypeArgumentListSyntax TypeArgumentList(IEnumerable<ITypeSymbol> typeArgs)
    {
        return TypeArgumentList(typeArgs.Select(x => IdentifierName(x.FullyQualifiedIdentifierName())));
    }

    public static ParameterListSyntax ParameterList(IEnumerable<IParameterSymbol> parameters)
    {
        var parameterSyntaxes = parameters.Select(Parameter);
        return SyntaxFactory.ParameterList(CommaSeparatedList(parameterSyntaxes));
    }

    public static ParameterListSyntax ParameterList(bool extensionMethod, IEnumerable<MethodParameter?> parameters)
    {
        var parameterSyntaxes = parameters
            .WhereNotNull()
            .DistinctBy(x => x.Ordinal)
            .OrderBy(x => x.Ordinal)
            .Select((p, i) => Parameter(extensionMethod && i == 0, p));
        return SyntaxFactory.ParameterList(CommaSeparatedList(parameterSyntaxes));
    }

    private static ParameterSyntax Parameter(bool addThisKeyword, MethodParameter parameter)
    {
        return Parameter(parameter.Type.FullyQualifiedIdentifierName(), parameter.Name, addThisKeyword);
    }

    private static ParameterSyntax Parameter(IParameterSymbol symbol)
    {
        var type = IdentifierName(symbol.Type.WithNullableAnnotation(symbol.NullableAnnotation.Upgrade()).FullyQualifiedIdentifierName())
            .AddTrailingSpace();
        var param = SyntaxFactory.Parameter(Identifier(symbol.Name)).WithType(type);

        if (symbol.IsThis)
        {
            param = param.WithModifiers(TokenList(TrailingSpacedToken(SyntaxKind.ThisKeyword)));
        }

        if (symbol.HasExplicitDefaultValue)
        {
            param = param.WithDefault(EqualsValueClause(Literal(symbol.ExplicitDefaultValue)));
        }

        return param;
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

    public InvocationExpressionSyntax StaticInvocation(string receiverType, string methodName, params ExpressionSyntax[] arguments) =>
        StaticInvocation(receiverType, methodName, arguments.Select(Argument));

    public InvocationExpressionSyntax StaticInvocation(string receiverType, string methodName, IEnumerable<ArgumentSyntax> arguments)
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

    public static ArgumentSyntax OutVarArgument(string name)
    {
        return Argument(DeclarationExpression(VarIdentifier, SingleVariableDesignation(Identifier(name))))
            .WithRefOrOutKeyword(TrailingSpacedToken(SyntaxKind.OutKeyword));
    }

    public static ArgumentListSyntax ArgumentListWithoutIndention(IEnumerable<ExpressionSyntax> argSyntaxes) =>
        ArgumentListWithoutIndention(argSyntaxes.Select(Argument));

    public static ArgumentListSyntax ArgumentListWithoutIndention(IEnumerable<ArgumentSyntax> argSyntaxes) =>
        SyntaxFactory.ArgumentList(CommaSeparatedList(argSyntaxes));

    private ArgumentListSyntax ArgumentList(IEnumerable<ExpressionSyntax> argSyntaxes) => ArgumentList(argSyntaxes.Select(Argument));

    private ArgumentListSyntax ArgumentList(IEnumerable<ArgumentSyntax> argSyntaxes) =>
        SyntaxFactory.ArgumentList(ConditionalCommaLineFeedSeparatedList(argSyntaxes));

    public static TypeArgumentListSyntax TypeArgumentList(params TypeSyntax[] argSyntaxes) =>
        SyntaxFactory.TypeArgumentList(CommaSeparatedList(argSyntaxes));

    public static TypeArgumentListSyntax TypeArgumentList(IEnumerable<TypeSyntax> argSyntaxes) =>
        SyntaxFactory.TypeArgumentList(CommaSeparatedList(argSyntaxes));
}
