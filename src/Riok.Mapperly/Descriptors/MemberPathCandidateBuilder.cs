using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public static class MemberPathCandidateBuilder
{
    internal static IEnumerable<IEnumerable<string>> BuildMemberPathCandidates(string name)
    {
        var chunks = StringChunker.ChunkPascalCase(name).ToList();
        for (var i = 1 << chunks.Count - 1; i > 0; i--)
        {
            yield return BuildName(chunks, i);
        }
    }

    private static IEnumerable<string> BuildName(IEnumerable<string> chunks, int splitPositions)
    {
        return chunks
            .Chunk((_, i) => (splitPositions & (1 << i)) == 0)
            .Select(x => string.Concat(x));
    }
}
