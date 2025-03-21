using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

// useful to create syntax factories: https://roslynquoter.azurewebsites.net/ and https://sharplab.io/
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SyntaxFactoryHelper(SupportedFeatures supportedFeatures)
{
    private const int ConditionalMultilineThreshold = 70;

    public static readonly IdentifierNameSyntax VarIdentifier = IdentifierName("var").AddTrailingSpace();

    private SyntaxFactoryHelper(int indentation, SupportedFeatures supportedFeatures)
        : this(supportedFeatures)
    {
        Indentation = indentation;
    }

    public int Indentation { get; }

    public SyntaxFactoryHelper AddIndentation() => new(Indentation + 1, supportedFeatures);

    public SyntaxFactoryHelper RemoveIndentation() => new(Indentation - 1, supportedFeatures);

    public static AssignmentExpressionSyntax Assignment(ExpressionSyntax target, ExpressionSyntax source, bool coalesce)
    {
        return coalesce ? CoalesceAssignment(target, source) : Assignment(target, source);
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
        return statement.IsKind(SyntaxKind.ReturnStatement) || statement.IsKind(SyntaxKind.ThrowStatement) ? statement : Block([statement]);
    }

    private BlockSyntax Block(IEnumerable<ExpressionSyntax> statements) => Block(statements.Select(AddIndentation().ExpressionStatement));

    public IReadOnlyCollection<StatementSyntax> SingleStatement(ExpressionSyntax expression) => [ExpressionStatement(expression)];

    public ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression) =>
        SyntaxFactory.ExpressionStatement(expression).AddLeadingLineFeed(Indentation);

    public static SeparatedSyntaxList<T> CommaSeparatedList<T>(IEnumerable<T> nodes, bool insertTrailingComma = false)
        where T : SyntaxNode
    {
        var sep = TrailingSpacedToken(SyntaxKind.CommaToken);
        var joinedNodes = Join(sep, insertTrailingComma, nodes);
        return SeparatedList<T>(joinedNodes);
    }

    public static SeparatedSyntaxList<T> CommaSeparatedList<T>(params T[] nodes)
        where T : SyntaxNode
    {
        var sep = TrailingSpacedToken(SyntaxKind.CommaToken);
        var joinedNodes = Join(sep, false, nodes);
        return SeparatedList<T>(joinedNodes);
    }

    private SeparatedSyntaxList<T> ConditionalCommaLineFeedSeparatedList<T>(IEnumerable<T> nodes)
        where T : SyntaxNode
    {
        var nodesList = nodes.ToList();
        SyntaxToken sep;
        if (nodesList.Sum(x => x.FullSpan.Length) < ConditionalMultilineThreshold)
        {
            sep = TrailingSpacedToken(SyntaxKind.CommaToken);
        }
        else
        {
            sep = TrailingLineFeedToken(SyntaxKind.CommaToken, Indentation + 1);
            nodesList = nodesList.Select(n => n.AddIndentation()).ToList();
            nodesList[0] = nodesList[0].AddLeadingLineFeed(Indentation + 1);
            nodesList[^1] = nodesList[^1].AddTrailingLineFeed(Indentation);
        }

        var joinedNodes = Join(sep, false, nodesList);
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
