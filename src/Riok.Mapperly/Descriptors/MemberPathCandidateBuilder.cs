using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public static class MemberPathCandidateBuilder
{
    /// <summary>
    /// Maximum number of indices which are considered to compute the member path candidates.
    /// </summary>
    private const int MaxPascalCaseIndices = 8;

    /// <summary>
    /// Splits a name into pascal case chunks and joins them together in all possible combinations.
    /// <example><c>"MyValueId"</c> leads to <c>[["MyValueId"], ["My", "ValueId"], ["MyValue", "Id"], ["My", "Value", "Id"]</c></example>
    /// </summary>
    /// <param name="name">The name to build candidates from.</param>
    /// <returns>The joined member path groups.</returns>
    public static IEnumerable<StringMemberPath> BuildMemberPathCandidates(string name)
    {
        if (name.Length == 0)
            yield break;

        // yield full string
        // as a fast path (often member match by their exact name)
        yield return new StringMemberPath([name]);

        var indices = GetPascalCaseSplitIndices(name).Take(MaxPascalCaseIndices).ToArray();
        if (indices.Length == 0)
            yield break;

        // try all permutations, skipping the first because the full string is already yielded
        var permutationsCount = 1 << indices.Length;
        for (var i = 1; i < permutationsCount; i++)
        {
            yield return new StringMemberPath(BuildPermutationParts(name, indices, i));
        }
    }

    /// <summary>
    /// Builds member path candidates based on the specified naming strategy.
    /// For SnakeCase strategy, converts the name from snake_case to PascalCase to match source members.
    /// Otherwise, delegates to the default BuildMemberPathCandidates method.
    /// </summary>
    /// <param name="name">The name to build candidates from.</param>
    /// <param name="strategy">The property name mapping strategy.</param>
    /// <returns>The joined member path groups.</returns>
    public static IEnumerable<StringMemberPath> BuildMemberPathCandidates(string name, PropertyNameMappingStrategy strategy)
    {
        if (strategy == PropertyNameMappingStrategy.SnakeCase)
        {
            if (name.Length == 0)
                yield break;

            // Convert from snake_case to PascalCase to match source members
            // e.g., "first_name" -> "FirstName"
            yield return new StringMemberPath([name.ToPascalCase()]);
        }
        else
        {
            // Use default behavior for other strategies
            foreach (var candidate in BuildMemberPathCandidates(name))
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<string> BuildPermutationParts(string source, int[] splitIndices, int enabledSplitPositions)
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
