using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit;

// created with the help of https://roslynquoter.azurewebsites.net/ and https://sharplab.io/
public static class SyntaxFactoryHelper
{
    private const string ArgumentOutOfRangeExceptionClassName = "System.ArgumentOutOfRangeException";
    private const string ArgumentNullExceptionClassName = "System.ArgumentNullException";
    private const string NotImplementedExceptionClassName = "System.NotImplementedException";
    private const string NullReferenceExceptionClassName = "System.NullReferenceException";

    public static readonly IdentifierNameSyntax VarIdentifier = IdentifierName("var");
    private static readonly SymbolDisplayFormat _fullyQualifiedNullableFormat = SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    private static readonly IdentifierNameSyntax _nameofIdentifier = IdentifierName("nameof");

    public static SyntaxToken Accessibility(Accessibility accessibility)
    {
        return accessibility switch
        {
            Microsoft.CodeAnalysis.Accessibility.Private => Token(SyntaxKind.PrivateKeyword),
            Microsoft.CodeAnalysis.Accessibility.Protected => Token(SyntaxKind.ProtectedKeyword),
            Microsoft.CodeAnalysis.Accessibility.Internal => Token(SyntaxKind.InternalKeyword),
            Microsoft.CodeAnalysis.Accessibility.Public => Token(SyntaxKind.PublicKeyword),
            _ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null)
        };
    }

    public static BinaryExpressionSyntax Coalesce(
        ExpressionSyntax expr,
        ExpressionSyntax coalesceExpr)
    {
        return BinaryExpression(
            SyntaxKind.CoalesceExpression,
            expr,
            coalesceExpr);
    }

    public static ExpressionSyntax Or(IEnumerable<ExpressionSyntax?> values)
        => values.WhereNotNull().Aggregate((a, b) => BinaryExpression(SyntaxKind.LogicalOrExpression, a, b));

    public static ExpressionSyntax And(params ExpressionSyntax?[] values)
        => And((IEnumerable<ExpressionSyntax?>)values);

    public static ExpressionSyntax And(IEnumerable<ExpressionSyntax?> values)
        => values.WhereNotNull().Aggregate((a, b) => BinaryExpression(SyntaxKind.LogicalAndExpression, a, b));

    public static ExpressionSyntax IfNoneNull(params (ITypeSymbol Type, ExpressionSyntax Access)[] values)
    {
        var conditions = values
            .Where(x => x.Type.IsNullable())
            .Select(x => IsNotNull(x.Access));
        return And(conditions);
    }

    public static ExpressionSyntax IfAnyNull(params (ITypeSymbol Type, ExpressionSyntax Access)[] values)
    {
        var conditions = values
            .Where(x => x.Type.IsNullable())
            .Select(x => IsNull(x.Access));
        return Or(conditions);
    }

    public static BinaryExpressionSyntax IsNull(ExpressionSyntax expression)
        => BinaryExpression(SyntaxKind.EqualsExpression, expression, NullLiteral());

    public static BinaryExpressionSyntax IsNotNull(ExpressionSyntax expression)
        => BinaryExpression(SyntaxKind.NotEqualsExpression, expression, NullLiteral());

    public static ExpressionSyntax NullSubstitute(ITypeSymbol t, ExpressionSyntax argument, NullFallbackValue nullFallbackValue)
    {
        return nullFallbackValue switch
        {
            NullFallbackValue.Default => DefaultLiteral(),
            NullFallbackValue.EmptyString => StringLiteral(string.Empty),
            NullFallbackValue.CreateInstance => CreateInstance(t),
            _ => ThrowArgumentNullException(argument),
        };
    }

    public static StatementSyntax IfNullReturnOrThrow(ExpressionSyntax expression, ExpressionSyntax? returnOrThrowExpression = null)
    {
        StatementSyntax ifExpression = returnOrThrowExpression switch
        {
            ThrowExpressionSyntax throwSyntax => ThrowStatement(throwSyntax.Expression),
            _ => ReturnStatement(returnOrThrowExpression),
        };

        return IfStatement(
            IsNull(expression),
            ifExpression);
    }

    public static LiteralExpressionSyntax DefaultLiteral()
        => LiteralExpression(SyntaxKind.DefaultLiteralExpression);

    public static LiteralExpressionSyntax NullLiteral()
        => LiteralExpression(SyntaxKind.NullLiteralExpression);

    public static LiteralExpressionSyntax StringLiteral(string content)
        => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(content));

    public static LiteralExpressionSyntax BooleanLiteral(bool b)
        => LiteralExpression(b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

    public static LiteralExpressionSyntax IntLiteral(int i)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i));

    public static StatementSyntax ReturnVariable(string identifierName)
        => ReturnStatement(IdentifierName(identifierName));

    public static MemberAccessExpressionSyntax MemberAccess(string identifierName, string propertyIdentifierName)
        => MemberAccess(IdentifierName(identifierName), propertyIdentifierName);

    public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax idExpression, string propertyIdentifierName)
        => MemberAccess(idExpression, IdentifierName(propertyIdentifierName));

    public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax idExpression, SimpleNameSyntax property)
        => MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, idExpression, property);

    public static AssignmentExpressionSyntax Assignment(
        ExpressionSyntax target,
        ExpressionSyntax source,
        SyntaxKind kind = SyntaxKind.SimpleAssignmentExpression)
        => AssignmentExpression(kind, target, source);

    public static ElementAccessExpressionSyntax ElementAccess(ExpressionSyntax idExpression, ExpressionSyntax index)
        => ElementAccessExpression(idExpression).WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(index))));

    public static ConditionalAccessExpressionSyntax ConditionalAccess(ExpressionSyntax idExpression, string propertyIdentifierName)
        => ConditionalAccessExpression(idExpression, MemberBindingExpression(IdentifierName(propertyIdentifierName)));

    public static InvocationExpressionSyntax NameOf(ExpressionSyntax expression)
        => Invocation(_nameofIdentifier, expression);

    public static ThrowExpressionSyntax ThrowNullReferenceException(string message)
    {
        return ThrowExpression(ObjectCreationExpression(IdentifierName(NullReferenceExceptionClassName))
            .WithArgumentList(ArgumentList(StringLiteral(message))));
    }

    public static ThrowExpressionSyntax ThrowArgumentOutOfRangeException(ExpressionSyntax arg, string message)
    {
        return ThrowExpression(ObjectCreationExpression(IdentifierName(ArgumentOutOfRangeExceptionClassName))
            .WithArgumentList(ArgumentList(NameOf(arg), arg, StringLiteral(message))));
    }

    public static ThrowExpressionSyntax ThrowArgumentNullException(ExpressionSyntax arg)
    {
        return ThrowExpression(ObjectCreationExpression(IdentifierName(ArgumentNullExceptionClassName))
            .WithArgumentList(ArgumentList(NameOf(arg))));
    }

    public static ThrowExpressionSyntax ThrowNotImplementedException()
    {
        return ThrowExpression(ObjectCreationExpression(IdentifierName(NotImplementedExceptionClassName))
            .WithArgumentList(SyntaxFactory.ArgumentList()));
    }

    public static InvocationExpressionSyntax GenericInvocation(string receiver, string methodName, IEnumerable<TypeSyntax> typeParams, params ExpressionSyntax[] arguments)
    {
        var method = GenericName(methodName)
            .WithTypeArgumentList(TypeArgumentList(typeParams.ToArray()));
        return InvocationExpression(MemberAccess(IdentifierName(receiver), method))
            .WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax GenericInvocation(string methodName, IEnumerable<TypeSyntax> typeParams, params ExpressionSyntax[] arguments)
    {
        var method = GenericName(methodName)
            .WithTypeArgumentList(TypeArgumentList(typeParams.ToArray()));
        return InvocationExpression(method)
            .WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax Invocation(string methodName, params MethodArgument?[] arguments)
        => Invocation(IdentifierName(methodName), arguments);

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method, params MethodArgument?[] arguments)
        => Invocation(method, arguments.WhereNotNull().OrderBy(x => x.Parameter.Ordinal).Select(x => x.Argument).ToArray());

    public static InvocationExpressionSyntax Invocation(string methodName, params ExpressionSyntax[] arguments)
        => Invocation(IdentifierName(methodName), arguments);

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method, params ExpressionSyntax[] arguments)
    {
        return InvocationExpression(method)
            .WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method)
        => Invocation(method, Array.Empty<ArgumentSyntax>());

    public static InvocationExpressionSyntax Invocation(ExpressionSyntax method, params ArgumentSyntax[] arguments)
    {
        return InvocationExpression(method)
            .WithArgumentList(ArgumentList(arguments));
    }

    public static ParameterListSyntax ParameterList(bool extensionMethod, params MethodParameter?[] parameters)
    {
        var parameterSyntaxes = parameters
            .WhereNotNull()
            .OrderBy(x => x.Ordinal)
            .Select(p => Parameter(extensionMethod, p));
        return SyntaxFactory.ParameterList(CommaSeparatedList(parameterSyntaxes));
    }

    public static ParameterSyntax Parameter(bool addThisKeyword, MethodParameter parameter)
    {
        var param = SyntaxFactory.Parameter(Identifier(parameter.Name))
            .WithType(FullyQualifiedIdentifier(parameter.Type));

        if (addThisKeyword && parameter.Ordinal == 0)
        {
            param = param.WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)));
        }

        return param;
    }

    public static InvocationExpressionSyntax StaticInvocation(string receiverType, string methodName, params ExpressionSyntax[] arguments)
    {
        var receiverTypeIdentifier = IdentifierName(receiverType);
        var methodAccess = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiverTypeIdentifier, IdentifierName(methodName));
        return InvocationExpression(methodAccess).WithArgumentList(ArgumentList(arguments));
    }

    public static InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ExpressionSyntax[] arguments)
        => StaticInvocation(
            FullyQualifiedIdentifierName(method.ReceiverType?.NonNullable()!) ?? throw new ArgumentNullException(nameof(method.ReceiverType)),
            method.Name,
            arguments);

    public static InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ArgumentSyntax[] arguments)
    {
        var receiverType = FullyQualifiedIdentifierName(method.ReceiverType?.NonNullable()!) ?? throw new ArgumentNullException(nameof(method.ReceiverType));

        var receiverTypeIdentifier = IdentifierName(receiverType);
        var methodAccess = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiverTypeIdentifier, IdentifierName(method.Name));
        return InvocationExpression(methodAccess).WithArgumentList(ArgumentList(arguments));
    }

    public static ForStatementSyntax IncrementalForLoop(string counterName, StatementSyntax body, ExpressionSyntax maxValueExclusive)
    {
        var counterDeclaration = DeclareVariable(counterName, IntLiteral(0));
        var counterIncrement = PostfixUnaryExpression(
            SyntaxKind.PostIncrementExpression,
            IdentifierName(counterName));
        var condition = BinaryExpression(
            SyntaxKind.LessThanExpression,
            IdentifierName(counterName),
            maxValueExclusive);
        return ForStatement(body)
            .WithDeclaration(counterDeclaration)
            .WithCondition(condition)
            .WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(counterIncrement));
    }

    public static VariableDeclarationSyntax DeclareVariable(string variableName, ExpressionSyntax initializationValue)
    {
        var initializer = EqualsValueClause(initializationValue);
        var declarator = VariableDeclarator(Identifier(variableName)).WithInitializer(initializer);
        return VariableDeclaration(VarIdentifier).WithVariables(SingletonSeparatedList(declarator));
    }

    public static LocalDeclarationStatementSyntax DeclareLocalVariable(string variableName, ExpressionSyntax initializationValue)
    {
        var variableDeclaration = DeclareVariable(variableName, initializationValue);
        return LocalDeclarationStatement(variableDeclaration);
    }

    public static StatementSyntax CreateInstance(string variableName, ITypeSymbol typeSymbol)
        => DeclareLocalVariable(variableName, CreateInstance(typeSymbol));

    public static ObjectCreationExpressionSyntax CreateInstance(ITypeSymbol typeSymbol)
    {
        var type = NonNullableIdentifier(typeSymbol);
        return ObjectCreationExpression(type).WithArgumentList(SyntaxFactory.ArgumentList());
    }

    public static ObjectCreationExpressionSyntax CreateInstance(ITypeSymbol typeSymbol, params ExpressionSyntax[] args)
    {
        var type = NonNullableIdentifier(typeSymbol);
        return ObjectCreationExpression(type).WithArgumentList(ArgumentList(args));
    }

    public static ObjectCreationExpressionSyntax CreateInstance(ITypeSymbol typeSymbol, params ArgumentSyntax[] args)
    {
        var type = NonNullableIdentifier(typeSymbol);
        return ObjectCreationExpression(type).WithArgumentList(ArgumentList(args));
    }

    public static InitializerExpressionSyntax ObjectInitializer(params ExpressionSyntax[] expressions)
    {
        return InitializerExpression(
            SyntaxKind.ObjectInitializerExpression,
            CommaSeparatedList(
                expressions));
    }

    public static SyntaxTrivia Nullable(bool enabled)
    {
        return Trivia(
            NullableDirectiveTrivia(
                Token(enabled ? SyntaxKind.EnableKeyword : SyntaxKind.DisableKeyword),
                true
            )
        );
    }

    public static NamespaceDeclarationSyntax Namespace(string ns)
        => NamespaceDeclaration(IdentifierName(ns));

    public static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argSyntaxes)
        => SyntaxFactory.ArgumentList(CommaSeparatedList(argSyntaxes.Select(Argument)));

    public static TypeArgumentListSyntax TypeArgumentList(params TypeSyntax[] argSyntaxes)
        => SyntaxFactory.TypeArgumentList(CommaSeparatedList(argSyntaxes));

    public static ArgumentListSyntax ArgumentList(params ArgumentSyntax[] args)
        => SyntaxFactory.ArgumentList(CommaSeparatedList(args));

    public static SeparatedSyntaxList<T> CommaSeparatedList<T>(IEnumerable<T> nodes, bool insertTrailingComma = false)
        where T : SyntaxNode
        => SeparatedList<T>(JoinByComma(nodes, insertTrailingComma));

    public static IdentifierNameSyntax NonNullableIdentifier(ITypeSymbol t)
        => FullyQualifiedIdentifier(t.NonNullable());

    public static IdentifierNameSyntax FullyQualifiedIdentifier(ITypeSymbol typeSymbol)
        => IdentifierName(FullyQualifiedIdentifierName(typeSymbol));

    public static string FullyQualifiedIdentifierName(ITypeSymbol typeSymbol)
        => typeSymbol.ToDisplayString(_fullyQualifiedNullableFormat);

    private static IEnumerable<SyntaxNodeOrToken> JoinByComma(IEnumerable<SyntaxNode> nodes, bool insertTrailingComma = false)
        => Join(Token(SyntaxKind.CommaToken), insertTrailingComma, nodes);

    private static IEnumerable<SyntaxNodeOrToken> Join(SyntaxToken sep, bool insertTrailingSeparator, IEnumerable<SyntaxNode> nodes)
    {
        var first = true;
        foreach (var node in nodes)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                yield return sep;
            }

            yield return node;
        }

        if (insertTrailingSeparator)
        {
            yield return sep;
        }
    }
}
