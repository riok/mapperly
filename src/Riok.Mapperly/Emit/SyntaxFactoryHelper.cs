using System.Text.RegularExpressions;
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
    private const string ArgumentExceptionClassName = "System.ArgumentException";
    private const string NotImplementedExceptionClassName = "System.NotImplementedException";
    private const string NullReferenceExceptionClassName = "System.NullReferenceException";

    public static readonly IdentifierNameSyntax VarIdentifier = IdentifierName("var");
    private static readonly SymbolDisplayFormat _fullyQualifiedNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
        );
    private static readonly IdentifierNameSyntax _nameofIdentifier = IdentifierName("nameof");

    private static readonly Regex FormattableStringPlaceholder = new Regex(@"\{(\d+)\}");

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

    public static BinaryExpressionSyntax Coalesce(ExpressionSyntax expr, ExpressionSyntax coalesceExpr) =>
        SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, expr, coalesceExpr);

    public static ExpressionSyntax Or(IEnumerable<ExpressionSyntax?> values) => BinaryExpression(SyntaxKind.LogicalOrExpression, values);

    public static ExpressionSyntax And(params ExpressionSyntax?[] values) => And((IEnumerable<ExpressionSyntax?>)values);

    public static ExpressionSyntax And(IEnumerable<ExpressionSyntax?> values) => BinaryExpression(SyntaxKind.LogicalAndExpression, values);

    public static ExpressionSyntax BitwiseAnd(params ExpressionSyntax?[] values) =>
        BinaryExpression(SyntaxKind.BitwiseAndExpression, values);

    public static ExpressionSyntax BitwiseOr(IEnumerable<ExpressionSyntax?> values) =>
        BinaryExpression(SyntaxKind.BitwiseOrExpression, values);

    public static PatternSyntax OrPattern(IEnumerable<ExpressionSyntax?> values) =>
        values
            .WhereNotNull()
            .Select<ExpressionSyntax, PatternSyntax>(ConstantPattern)
            .Aggregate((left, right) => BinaryPattern(SyntaxKind.OrPattern, left, right));

    public static ExpressionSyntax Equal(ExpressionSyntax left, ExpressionSyntax right) =>
        BinaryExpression(SyntaxKind.EqualsExpression, left, right);

    public static ExpressionSyntax IfNoneNull(params (ITypeSymbol Type, ExpressionSyntax Access)[] values)
    {
        var conditions = values.Where(x => x.Type.IsNullable()).Select(x => IsNotNull(x.Access));
        return And(conditions);
    }

    public static ExpressionSyntax IfAnyNull(params (ITypeSymbol Type, ExpressionSyntax Access)[] values)
    {
        var conditions = values.Where(x => x.Type.IsNullable()).Select(x => IsNull(x.Access));
        return Or(conditions);
    }

    public static BinaryExpressionSyntax IsNull(ExpressionSyntax expression) =>
        SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, expression, NullLiteral());

    public static BinaryExpressionSyntax IsNotNull(ExpressionSyntax expression) =>
        SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, expression, NullLiteral());

    public static ExpressionSyntax NullSubstitute(ITypeSymbol t, ExpressionSyntax argument, NullFallbackValue nullFallbackValue)
    {
        return nullFallbackValue switch
        {
            NullFallbackValue.Default => DefaultLiteral(),
            NullFallbackValue.EmptyString => StringLiteral(string.Empty),
            NullFallbackValue.CreateInstance => CreateInstance(t),
            _ when argument is ElementAccessExpressionSyntax memberAccess
                => ThrowNullReferenceException(
                    InterpolatedString(
                        $"Sequence {NameOf(memberAccess.Expression)}, contained a null value at index {memberAccess.ArgumentList.Arguments[0].Expression}."
                    )
                ),
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

        return IfStatement(IsNull(expression), ifExpression);
    }

    public static InterpolatedStringExpressionSyntax InterpolatedString(FormattableString str)
    {
        var matches = FormattableStringPlaceholder.Matches(str.Format);
        var contents = new List<InterpolatedStringContentSyntax>();
        var previousIndex = 0;
        foreach (Match match in matches)
        {
            var text = str.Format.Substring(previousIndex, match.Index - previousIndex);
            contents.Add(InterpolatedStringText(text));

            var arg = str.GetArgument(int.Parse(match.Groups[1].Value));
            InterpolatedStringContentSyntax argSyntax = arg switch
            {
                ExpressionSyntax x => Interpolation(x),
                string x => InterpolatedStringText(x),
                _ => throw new InvalidOperationException(arg.GetType() + " cannot be converted into a string interpolation"),
            };
            contents.Add(argSyntax);
            previousIndex = match.Index + match.Length;
        }

        if (previousIndex <= str.Format.Length)
        {
            contents.Add(InterpolatedStringText(str.Format.Substring(previousIndex)));
        }

        return InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken)).WithContents(List(contents));
    }

    public static LiteralExpressionSyntax DefaultLiteral() => LiteralExpression(SyntaxKind.DefaultLiteralExpression);

    public static LiteralExpressionSyntax NullLiteral() => LiteralExpression(SyntaxKind.NullLiteralExpression);

    public static LiteralExpressionSyntax StringLiteral(string content) =>
        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(content));

    public static LiteralExpressionSyntax BooleanLiteral(bool b) =>
        LiteralExpression(b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

    public static LiteralExpressionSyntax IntLiteral(int i) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i));

    public static StatementSyntax ReturnVariable(string identifierName) => ReturnStatement(IdentifierName(identifierName));

    public static MemberAccessExpressionSyntax MemberAccess(string identifierName, string propertyIdentifierName) =>
        MemberAccess(IdentifierName(identifierName), propertyIdentifierName);

    public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax idExpression, string propertyIdentifierName) =>
        MemberAccess(idExpression, IdentifierName(propertyIdentifierName));

    public static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax idExpression, SimpleNameSyntax property) =>
        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, idExpression, property);

    public static AssignmentExpressionSyntax Assignment(
        ExpressionSyntax target,
        ExpressionSyntax source,
        SyntaxKind kind = SyntaxKind.SimpleAssignmentExpression
    ) => AssignmentExpression(kind, target, source);

    public static ElementAccessExpressionSyntax ElementAccess(ExpressionSyntax idExpression, ExpressionSyntax index) =>
        ElementAccessExpression(idExpression).WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(index))));

    public static ConditionalAccessExpressionSyntax ConditionalAccess(ExpressionSyntax idExpression, string propertyIdentifierName) =>
        ConditionalAccessExpression(idExpression, MemberBindingExpression(IdentifierName(propertyIdentifierName)));

    public static InvocationExpressionSyntax NameOf(ExpressionSyntax expression) => Invocation(_nameofIdentifier, expression);

    public static ThrowExpressionSyntax ThrowNullReferenceException(string message)
    {
        return ThrowExpression(
            ObjectCreationExpression(IdentifierName(NullReferenceExceptionClassName)).WithArgumentList(ArgumentList(StringLiteral(message)))
        );
    }

    public static ThrowExpressionSyntax ThrowNullReferenceException(ExpressionSyntax arg)
    {
        return ThrowExpression(
            ObjectCreationExpression(IdentifierName(NullReferenceExceptionClassName)).WithArgumentList(ArgumentList(arg))
        );
    }

    public static ThrowExpressionSyntax ThrowArgumentOutOfRangeException(ExpressionSyntax arg, string message)
    {
        return ThrowExpression(
            ObjectCreationExpression(IdentifierName(ArgumentOutOfRangeExceptionClassName))
                .WithArgumentList(ArgumentList(NameOf(arg), arg, StringLiteral(message)))
        );
    }

    public static ThrowExpressionSyntax ThrowArgumentNullException(ExpressionSyntax arg)
    {
        return ThrowExpression(
            ObjectCreationExpression(IdentifierName(ArgumentNullExceptionClassName)).WithArgumentList(ArgumentList(NameOf(arg)))
        );
    }

    public static ThrowExpressionSyntax ThrowArgumentExpression(ExpressionSyntax message, ExpressionSyntax arg)
    {
        return ThrowExpression(
            ObjectCreationExpression(IdentifierName(ArgumentExceptionClassName)).WithArgumentList(ArgumentList(message, NameOf(arg)))
        );
    }

    public static ThrowExpressionSyntax ThrowNotImplementedException()
    {
        return ThrowExpression(
            ObjectCreationExpression(IdentifierName(NotImplementedExceptionClassName)).WithArgumentList(SyntaxFactory.ArgumentList())
        );
    }

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

    public static ParameterSyntax Parameter(bool addThisKeyword, MethodParameter parameter)
    {
        var param = SyntaxFactory.Parameter(Identifier(parameter.Name)).WithType(FullyQualifiedIdentifier(parameter.Type));

        if (addThisKeyword && parameter.Ordinal == 0)
        {
            param = param.WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)));
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
        var receiver = method.ReceiverType ?? throw new NullReferenceException(nameof(method.ReceiverType) + " is null");
        var qualifiedReceiverName = FullyQualifiedIdentifierName(receiver.NonNullable());
        return $"{qualifiedReceiverName}.{method.Name}";
    }

    public static InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ExpressionSyntax[] arguments)
    {
        var receiver = method.ReceiverType ?? throw new NullReferenceException(nameof(method.ReceiverType) + " is null");
        var qualifiedReceiverName = FullyQualifiedIdentifierName(receiver.NonNullable());
        return StaticInvocation(qualifiedReceiverName, method.Name, arguments);
    }

    public static InvocationExpressionSyntax StaticInvocation(IMethodSymbol method, params ArgumentSyntax[] arguments)
    {
        var receiver = method.ReceiverType ?? throw new NullReferenceException(nameof(method.ReceiverType) + " is null");
        var qualifiedReceiverName = FullyQualifiedIdentifierName(receiver.NonNullable());

        var receiverTypeIdentifier = IdentifierName(qualifiedReceiverName);
        var methodAccess = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            receiverTypeIdentifier,
            IdentifierName(method.Name)
        );
        return InvocationExpression(methodAccess).WithArgumentList(ArgumentList(arguments));
    }

    public static ForStatementSyntax IncrementalForLoop(string counterName, StatementSyntax body, ExpressionSyntax maxValueExclusive)
    {
        var counterDeclaration = DeclareVariable(counterName, IntLiteral(0));
        var counterIncrement = PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName(counterName));
        var condition = BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName(counterName), maxValueExclusive);
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

    public static StatementSyntax CreateInstance(string variableName, ITypeSymbol typeSymbol) =>
        DeclareLocalVariable(variableName, CreateInstance(typeSymbol));

    public static ObjectCreationExpressionSyntax CreateInstance(string typeName)
    {
        var type = IdentifierName(typeName);
        return ObjectCreationExpression(type).WithArgumentList(SyntaxFactory.ArgumentList());
    }

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
        return InitializerExpression(SyntaxKind.ObjectInitializerExpression, CommaSeparatedList(expressions));
    }

    public static SyntaxTrivia Nullable(bool enabled)
    {
        return Trivia(NullableDirectiveTrivia(Token(enabled ? SyntaxKind.EnableKeyword : SyntaxKind.DisableKeyword), true));
    }

    public static NamespaceDeclarationSyntax Namespace(string ns) => NamespaceDeclaration(IdentifierName(ns));

    public static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argSyntaxes) =>
        SyntaxFactory.ArgumentList(CommaSeparatedList(argSyntaxes.Select(Argument)));

    public static TypeArgumentListSyntax TypeArgumentList(params TypeSyntax[] argSyntaxes) =>
        SyntaxFactory.TypeArgumentList(CommaSeparatedList(argSyntaxes));

    public static ArgumentListSyntax ArgumentList(params ArgumentSyntax[] args) => SyntaxFactory.ArgumentList(CommaSeparatedList(args));

    public static SeparatedSyntaxList<T> CommaSeparatedList<T>(IEnumerable<T> nodes, bool insertTrailingComma = false)
        where T : SyntaxNode => SeparatedList<T>(JoinByComma(nodes, insertTrailingComma));

    public static IdentifierNameSyntax NonNullableIdentifier(ITypeSymbol t) => FullyQualifiedIdentifier(t.NonNullable());

    public static IdentifierNameSyntax FullyQualifiedIdentifier(ITypeSymbol typeSymbol) =>
        IdentifierName(FullyQualifiedIdentifierName(typeSymbol));

    public static string FullyQualifiedIdentifierName(ITypeSymbol typeSymbol) => typeSymbol.ToDisplayString(_fullyQualifiedNullableFormat);

    public static IReadOnlyCollection<StatementSyntax> SingleStatement(ExpressionSyntax expression) =>
        new[] { ExpressionStatement(expression) };

    private static ExpressionSyntax BinaryExpression(SyntaxKind kind, params ExpressionSyntax?[] values) =>
        BinaryExpression(kind, (IEnumerable<ExpressionSyntax?>)values);

    private static ExpressionSyntax BinaryExpression(SyntaxKind kind, IEnumerable<ExpressionSyntax?> values) =>
        values.WhereNotNull().Aggregate((left, right) => SyntaxFactory.BinaryExpression(kind, left, right));

    private static InterpolatedStringTextSyntax InterpolatedStringText(string text) =>
        SyntaxFactory.InterpolatedStringText(
            Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken, text, text, SyntaxTriviaList.Empty)
        );

    private static IEnumerable<SyntaxNodeOrToken> JoinByComma(IEnumerable<SyntaxNode> nodes, bool insertTrailingComma = false) =>
        Join(Token(SyntaxKind.CommaToken), insertTrailingComma, nodes);

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
