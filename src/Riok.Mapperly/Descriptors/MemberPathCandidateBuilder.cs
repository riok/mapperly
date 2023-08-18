namespace Riok.Mapperly.Descriptors;

public static class MemberPathCandidateBuilder
{
    /// <summary>
    /// Splits a name into pascal case chunks and joins them together in all possible combinations.
    /// <example><c>"MyValueId"</c> leads to <c>[["MyValueId"], ["My", "ValueId"], ["MyValue", "Id"], ["My", "Value", "Id"]</c></example>
    /// </summary>
    /// <param name="name">The name to build candidates from.</param>
    /// <returns>The joined member path groups.</returns>
    public static IEnumerable<IEnumerable<string>> BuildMemberPathCandidates(string name)
    {
        if (name.Length == 0)
            yield break;

        // yield full string
        yield return new[] { name };

        var indices = GetPascalCaseSplitIndices(name).ToArray();

        // try all permutations, skipping the first because the full string is already yielded
        var permutationsCount = 1 << indices.Length;
        for (var i = 1; i < permutationsCount; i++)
        {
            yield return BuildName(name, indices, i);
        }
    }

    private static IEnumerable<string> BuildName(string source, int[] splitIndices, int enabledSplitPositions)
    {
        var lastSplitIndex = 0;
        var currentSplitPosition = 1;
        foreach (var splitIndex in splitIndices)
        {
            if ((enabledSplitPositions & currentSplitPosition) == currentSplitPosition)
            {
                yield return source.Substring(lastSplitIndex, splitIndex - lastSplitIndex);
                lastSplitIndex = splitIndex;
            }

            currentSplitPosition <<= 1;
        }

        if (lastSplitIndex < source.Length)
            yield return source.Substring(lastSplitIndex);
    }

    private static IEnumerable<int> GetPascalCaseSplitIndices(string str)
    {
        for (var i = 1; i < str.Length; i++)
        {
            if (char.IsUpper(str[i]))
                yield return i;
        }
    }
}
