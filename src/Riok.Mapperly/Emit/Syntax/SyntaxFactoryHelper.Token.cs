using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    private static readonly SyntaxTriviaList _spaceTriviaList = SyntaxTriviaList.Create(ElasticSpace);

    private SyntaxToken LeadingLineFeedToken(SyntaxKind kind)
    {
        return Token(SyntaxTriviaList.Empty.AddLineFeedAndIndentation(Indentation), kind, SyntaxTriviaList.Empty);
    }

    private SyntaxToken LeadingLineFeedTrailingSpaceToken(SyntaxKind kind)
    {
        return Token(SyntaxTriviaList.Empty.AddLineFeedAndIndentation(Indentation), kind, _spaceTriviaList);
    }

    private static SyntaxToken LeadingSpacedToken(SyntaxKind kind) => Token(_spaceTriviaList, kind, SyntaxTriviaList.Empty);

    public static SyntaxToken TrailingSpacedToken(SyntaxKind kind) => Token(SyntaxTriviaList.Empty, kind, _spaceTriviaList);

    private static SyntaxToken SpacedToken(SyntaxKind kind) => Token(_spaceTriviaList, kind, _spaceTriviaList);
}
