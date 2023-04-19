using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public static class MemberPathCandidateBuilder
{
    /// <summary>
    /// Splits a name into pascal case chunks and joins them together in all possible combinations.
    /// <example><c>"MyValueId"</c> leads to <c>[["MyValueId"], ["My", "ValueId"], ["MyValue", "Id"], ["My", "Value", "Id"]</c></example>
    /// </summary>
    /// <param name="name">The name to build candidates from.</param>
    /// <returns>The joined member path groups.</returns>
    internal static IEnumerable<IEnumerable<string>> BuildMemberPathCandidates(string name)
    {
        var chunks = StringChunker.ChunkPascalCase(name).ToList();
        for (var i = 1 << chunks.Count - 1; i > 0; i--)
        {
            yield return BuildName(chunks, i - 1);
        }
    }

    private static IEnumerable<string> BuildName(IEnumerable<string> chunks, int splitPositions)
    {
        return chunks.Chunk((_, i) => (splitPositions & (1 << i)) == 0).Select(x => string.Concat(x));
    }
}
