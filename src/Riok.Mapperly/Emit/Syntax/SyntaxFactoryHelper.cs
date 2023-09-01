using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

// useful to create syntax factories: https://roslynquoter.azurewebsites.net/ and https://sharplab.io/
public readonly partial struct SyntaxFactoryHelper
{
    public static readonly IdentifierNameSyntax VarIdentifier = IdentifierName("var").AddTrailingSpace();

    private readonly string _assemblyName;

    public SyntaxFactoryHelper(string assemblyName)
    {
        _assemblyName = assemblyName;
    }

    private SyntaxFactoryHelper(int indentation, string assemblyName)
    {
        Indentation = indentation;
        _assemblyName = assemblyName;
    }

    public int Indentation { get; }

    public SyntaxFactoryHelper AddIndentation() => new(Indentation + 1, _assemblyName);

    public SyntaxFactoryHelper RemoveIndentation() => new(Indentation - 1, _assemblyName);

    public static SyntaxToken Accessibility(Accessibility accessibility)
    {
        return accessibility switch
        {
            Microsoft.CodeAnalysis.Accessibility.Private => TrailingSpacedToken(SyntaxKind.PrivateKeyword),
            Microsoft.CodeAnalysis.Accessibility.Protected => TrailingSpacedToken(SyntaxKind.ProtectedKeyword),
            Microsoft.CodeAnalysis.Accessibility.Internal => TrailingSpacedToken(SyntaxKind.InternalKeyword),
            Microsoft.CodeAnalysis.Accessibility.Public => TrailingSpacedToken(SyntaxKind.PublicKeyword),
            _ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null)
        };
    }

    public static AssignmentExpressionSyntax Assignment(ExpressionSyntax target, ExpressionSyntax source)
    {
        return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, target, SpacedToken(SyntaxKind.EqualsToken), source);
    }

    public BlockSyntax Block(IEnumerable<StatementSyntax> statements)
    {
        return SyntaxFactory
            .Block(statements)
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken));
    }

    private StatementSyntax BlockIfNotReturnOrThrow(StatementSyntax statement)
    {
        return statement.IsKind(SyntaxKind.ReturnStatement) || statement.IsKind(SyntaxKind.ThrowStatement)
            ? statement
            : Block(new[] { statement });
    }

    private BlockSyntax Block(IEnumerable<ExpressionSyntax> statements) => Block(statements.Select(AddIndentation().ExpressionStatement));

    public IReadOnlyCollection<StatementSyntax> SingleStatement(ExpressionSyntax expression) => new[] { ExpressionStatement(expression) };

    public ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression) =>
        SyntaxFactory.ExpressionStatement(expression).AddLeadingLineFeed(Indentation);

    public static SeparatedSyntaxList<T> CommaSeparatedList<T>(IEnumerable<T> nodes, bool insertTrailingComma = false)
        where T : SyntaxNode
    {
        var sep = TrailingSpacedToken(SyntaxKind.CommaToken);
        var joinedNodes = Join(sep, insertTrailingComma, nodes);
        return SeparatedList<T>(joinedNodes);
    }

    private SeparatedSyntaxList<T> CommaLineFeedSeparatedList<T>(IEnumerable<T> nodes)
        where T : SyntaxNode
    {
        // append a comma at the end but no line feed
        var sep = Token(SyntaxKind.CommaToken).AddTrailingLineFeed(Indentation);
        var joinedNodes = Join(sep, false, nodes).Append(Token(SyntaxKind.CommaToken));
        return SeparatedList<T>(joinedNodes);
    }

    public static VariableDeclarationSyntax DeclareVariable(string variableName, ExpressionSyntax initializationValue)
    {
        var initializer = EqualsValueClause(SpacedToken(SyntaxKind.EqualsToken), initializationValue);
        var declarator = VariableDeclarator(Identifier(variableName)).WithInitializer(initializer);
        return VariableDeclaration(VarIdentifier).WithVariables(SingletonSeparatedList(declarator));
    }

    public LocalDeclarationStatementSyntax DeclareLocalVariable(string variableName, ExpressionSyntax initializationValue)
    {
        var variableDeclaration = DeclareVariable(variableName, initializationValue);
        return LocalDeclarationStatement(variableDeclaration).AddLeadingLineFeed(Indentation);
    }

    public static NameColonSyntax SpacedNameColon(string name) =>
        NameColon(IdentifierName(name), TrailingSpacedToken(SyntaxKind.ColonToken));

    private static IEnumerable<SyntaxNodeOrToken> Join(SyntaxToken sep, bool insertTrailingSeparator, IEnumerable<SyntaxNode> nodes)
    {
        using var enumerator = nodes.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        yield return enumerator.Current;

        while (enumerator.MoveNext())
        {
            yield return sep;
            yield return enumerator.Current;
        }

        if (insertTrailingSeparator)
        {
            yield return sep;
        }
    }
}
