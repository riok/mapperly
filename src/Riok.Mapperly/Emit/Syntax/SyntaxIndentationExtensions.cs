using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

internal static class SyntaxIndentationExtensions
{
    private static readonly SyntaxTrivia _indentation = ElasticWhitespace("    ");
    private static readonly ConcurrentDictionary<int, SyntaxTrivia[]> _indentationLineFeedSyntaxTriviaCache = new();

    public static IEnumerable<TSyntax> SeparateByLineFeed<TSyntax>(this IEnumerable<TSyntax> syntax, int indentation)
        where TSyntax : SyntaxNode
    {
        using var enumerator = syntax.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        yield return enumerator.Current!.AddLeadingLineFeed(indentation);

        while (enumerator.MoveNext())
        {
            var node = enumerator.Current!;
            yield return node.AddLeadingLineFeed(indentation).AddLeadingLineFeed(0);
        }
    }

    public static IEnumerable<TSyntax> SeparateByTrailingLineFeed<TSyntax>(this IEnumerable<TSyntax> syntax, int indentation)
        where TSyntax : SyntaxNode
    {
        using var enumerator = syntax.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        var current = enumerator.Current!;

        while (enumerator.MoveNext())
        {
            yield return current.AddTrailingLineFeed(indentation);
            current = enumerator.Current!;
        }

        yield return current;
    }

    /// <summary>
    /// Adds a leading line feed to the first found trivia.
    /// If the first token is known by the caller, use <see cref="AddLeadingLineFeed"/> for the first token instead
    /// (should have better performance)
    /// </summary>
    /// <param name="syntax">The syntax.</param>
    /// <param name="indentation">The indentation.</param>
    /// <typeparam name="TSyntax">The type of the syntax.</typeparam>
    /// <returns>The updated syntax.</returns>
    public static TSyntax AddLeadingLineFeed<TSyntax>(this TSyntax syntax, int indentation)
        where TSyntax : SyntaxNode
    {
        var trivia = syntax.GetLeadingTrivia();
        trivia = trivia.AddLineFeedAndIndentation(indentation);
        return syntax.WithLeadingTrivia(trivia);
    }

    /// <summary>
    /// Adds a trailing line feed to the last found trivia.
    /// If the last token is known by the caller, use <see cref="AddTrailingLineFeed"/> for the last token instead
    /// (should have better performance).
    /// </summary>
    /// <param name="syntax">The syntax.</param>
    /// <param name="indentation">The indentation.</param>
    /// <typeparam name="TSyntax">The type of the syntax.</typeparam>
    /// <returns>The updated syntax.</returns>
    public static TSyntax AddTrailingLineFeed<TSyntax>(this TSyntax syntax, int indentation)
        where TSyntax : SyntaxNode
    {
        var trivia = syntax.GetTrailingTrivia();
        trivia = trivia.AddLineFeedAndIndentation(indentation);
        return syntax.WithTrailingTrivia(trivia);
    }

    public static TSyntax AddLeadingSpace<TSyntax>(this TSyntax syntax)
        where TSyntax : SyntaxNode => syntax.WithLeadingTrivia(syntax.GetLeadingTrivia().Add(ElasticSpace));

    public static TSyntax AddTrailingSpace<TSyntax>(this TSyntax syntax)
        where TSyntax : SyntaxNode => syntax.WithTrailingTrivia(syntax.GetTrailingTrivia().Add(ElasticSpace));

    public static SyntaxToken AddLeadingLineFeed(this SyntaxToken token, int indentation)
    {
        var trivia = token.LeadingTrivia.AddLineFeedAndIndentation(indentation);
        return token.WithLeadingTrivia(trivia);
    }

    public static SyntaxToken AddTrailingLineFeed(this SyntaxToken token, int indentation)
    {
        var trivia = token.TrailingTrivia.AddLineFeedAndIndentation(indentation);
        return token.WithTrailingTrivia(trivia);
    }

    public static SyntaxTriviaList AddLineFeedAndIndentation(this SyntaxTriviaList trivia, int indentation)
    {
        var triviaToInsert = _indentationLineFeedSyntaxTriviaCache.GetOrAdd(
            indentation,
            static indentationLevel =>
            {
                // +1 for the new line at the beginning of the indentation
                var toInsert = new SyntaxTrivia[indentationLevel + 1];
                toInsert[0] = ElasticCarriageReturnLineFeed;
                for (var i = 1; i < toInsert.Length; i++)
                {
                    toInsert[i] = _indentation;
                }

                return toInsert;
            }
        );

        return trivia.InsertRange(0, triviaToInsert);
    }

    public static T AddIndentation<T>(this T n)
        where T : SyntaxNode => IndentationRewriter.Rewrite(n);

    private class IndentationRewriter : CSharpSyntaxRewriter
    {
        private static readonly IndentationRewriter _instance = new();

        private IndentationRewriter() { }

        public static T Rewrite<T>(T node)
            where T : SyntaxNode
        {
            return (T)_instance.Visit(node);
        }

        public override SyntaxTriviaList VisitList(SyntaxTriviaList list)
        {
            if (list.Count == 0)
                return list;

            var idx = -1;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                idx++;

                if (!enumerator.Current.IsKind(SyntaxKind.EndOfLineTrivia))
                    continue;

                var newList = new List<SyntaxTrivia>(list.Count);
                newList.AddRange(list.Take(idx + 1));
                newList.Add(_indentation);
                VisitRemainingList(enumerator, newList);
                return TriviaList(newList);
            }

            return list;
        }

        private static void VisitRemainingList(SyntaxTriviaList.Enumerator enumerator, List<SyntaxTrivia> list)
        {
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);

                if (enumerator.Current.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    list.Add(_indentation);
                }
            }
        }
    }
}
