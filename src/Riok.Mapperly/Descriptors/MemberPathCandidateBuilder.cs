using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public static class MemberPathCandidateBuilder
{
    /// <summary>
    /// Maximum number of indices which are considered to compute the member path candidates.
    /// </summary>
    private const int MaxPermutationIndices = 8;

    /// <summary>
    /// Splits a name into chunks and joins them together in all possible combinations.
    /// <example><c>"MyValueId"</c> leads to <c>[["MyValueId"], ["My", "ValueId"], ["MyValue", "Id"], ["My", "Value", "Id"]</c></example>
    /// </summary>
    /// <param name="name">The name to build candidates from.</param>
    /// <param name="strategy">The naming strategy to use.</param>
    /// <returns>The joined member path groups.</returns>
    public static IEnumerable<StringMemberPath> BuildMemberPathCandidates(string name, PropertyNameMappingStrategy strategy)
    {
        if (string.IsNullOrEmpty(name))
            return [];

        return BuildCandidates(name, strategy).Prepend(new StringMemberPath([name])).DistinctBy(x => x.FullName);
    }

    private static IEnumerable<StringMemberPath> BuildCandidates(string name, PropertyNameMappingStrategy strategy)
    {
        return strategy switch
        {
            PropertyNameMappingStrategy.SnakeCase => BuildSnakeCaseCandidates(name, name.ToSnakeCase()),
            PropertyNameMappingStrategy.UpperSnakeCase => BuildSnakeCaseCandidates(name, name.ToUpperSnakeCase()),
            _ => BuildPermutations(name, char.IsUpper, skipSeparator: false),
        };
    }

    private static IEnumerable<StringMemberPath> BuildSnakeCaseCandidates(string originalName, string snakeCaseName)
    {
        var snakeCasePermutations = BuildPermutations(snakeCaseName, static c => c == '_', skipSeparator: true);

        // PascalCase Fallback
        var pascalCaseSource = originalName.Contains('_', StringComparison.Ordinal) ? originalName.ToPascalCase() : originalName;
        var pascalCasePermutations = BuildCandidates(pascalCaseSource, PropertyNameMappingStrategy.CaseSensitive);
        return snakeCasePermutations.Concat(pascalCasePermutations);
    }

    private static IEnumerable<StringMemberPath> BuildPermutations(string name, Func<char, bool> isSeparator, bool skipSeparator)
    {
        var indices = GetSplitIndices(name, isSeparator).Take(MaxPermutationIndices).ToArray();

        // try all permutations
        var permutationsCount = 1 << indices.Length;
        for (var i = 0; i < permutationsCount; i++)
        {
            yield return new StringMemberPath(BuildPermutationParts(name, indices, i, skipSeparator));
        }
    }

    private static IEnumerable<string> BuildPermutationParts(
        string source,
        int[] splitIndices,
        int enabledSplitPositions,
        bool skipSplitIndex
    )
    {
        var lastSplitIndex = 0;
        var currentSplitPosition = 1;
        foreach (var splitIndex in splitIndices)
        {
            if ((enabledSplitPositions & currentSplitPosition) == currentSplitPosition)
            {
                yield return source.Substring(lastSplitIndex, splitIndex - lastSplitIndex);
                lastSplitIndex = splitIndex + (skipSplitIndex ? 1 : 0);
            }

            currentSplitPosition <<= 1;
        }

        if (lastSplitIndex < source.Length)
            yield return source.Substring(lastSplitIndex);
    }

    private static IEnumerable<int> GetSplitIndices(string str, Func<char, bool> isSeparator)
    {
        for (var i = 1; i < str.Length; i++)
        {
            if (isSeparator(str[i]))
                yield return i;
        }
    }
}
