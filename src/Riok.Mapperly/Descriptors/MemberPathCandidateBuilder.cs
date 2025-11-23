using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public static class MemberPathCandidateBuilder
{
    // Limit recursion/complexity (8 indices = 256 permutations max)
    private const int MaxIndices = 8;

    public static IEnumerable<StringMemberPath> BuildMemberPathCandidates(string name, PropertyNameMappingStrategy strategy)
    {
        if (string.IsNullOrEmpty(name))
        {
            yield break;
        }

        // 1. Always yield the exact match first (Fastest path)
        yield return new StringMemberPath(new[] { name });

        switch (strategy)
        {
            case PropertyNameMappingStrategy.CaseSensitive:
                foreach (var item in BuildCaseSensitivePermutations(name))
                    yield return item;
                break;

            case PropertyNameMappingStrategy.CaseInsensitive:
                // CaseSensitive permutations include the PascalCase splits
                foreach (var item in BuildCaseSensitivePermutations(name))
                    yield return item;

                // Lower/Upper fallbacks
                // Verify they are different from original to avoid dups (e.g. "id".ToLower() == "id")
                var lower = name.ToLowerInvariant();
                if (!string.Equals(lower, name, StringComparison.Ordinal))
                    yield return new StringMemberPath(new[] { lower });

                var upper = name.ToUpperInvariant();
                if (!string.Equals(upper, name, StringComparison.Ordinal) && !string.Equals(upper, lower, StringComparison.Ordinal))
                    yield return new StringMemberPath(new[] { upper });
                break;

            case PropertyNameMappingStrategy.SnakeCase:
                foreach (var item in BuildSnakeCaseCandidates(name))
                    yield return item;
                break;

            case PropertyNameMappingStrategy.UpperSnakeCase:
                foreach (var item in BuildUpperSnakeCaseCandidates(name))
                    yield return item;
                break;

            default:
                foreach (var item in BuildCaseSensitivePermutations(name))
                    yield return item;
                break;
        }
    }

    private static IEnumerable<StringMemberPath> BuildCaseSensitivePermutations(string name)
    {
        var indices = new int[MaxIndices];
        var indicesCount = FillPascalSplitIndices(name, indices);

        if (indicesCount == 0)
            yield break;

        var permutationsCount = 1 << indicesCount;

        // Start at 1. Mask 0 is "no splits", which equals the original string.
        for (var i = 1; i < permutationsCount; i++)
        {
            yield return new StringMemberPath(BuildPermutationList(name, indices, indicesCount, i, skipDelimiter: false));
        }
    }

    private static IEnumerable<StringMemberPath> BuildSnakeCaseCandidates(string name)
    {
        // 1. Variant: Strict Snake Case
        // Only generate if input wasn't already valid snake case
        if (!IsSnakeCase(name))
        {
            yield return new StringMemberPath(new[] { name.ToSnakeCase() });
        }

        // 2. Permutations based on underscores (e.g. "order_id" -> "order.id")
        foreach (var candidate in GenerateSnakeCasePermutations(name))
        {
            yield return candidate;
        }

        // 3. Fallback: PascalCase integration
        // If we have "customer_name" (source) mapping to "CustomerName" (target)
        // We convert source to Pascal, then try Pascal permutations
        var pascalName = name.ToPascalCase();

        // Optimize: Don't run if ToPascalCase returned the exact same string (it was already pascal)
        if (string.Equals(pascalName, name, StringComparison.Ordinal))
        {
            yield break;
        }

        yield return new StringMemberPath(new[] { pascalName });
        foreach (var box in BuildCaseSensitivePermutations(pascalName))
        {
            yield return box;
        }
    }

    private static IEnumerable<StringMemberPath> BuildUpperSnakeCaseCandidates(string name)
    {
        // 1. Variant: Upper Snake
        // Optimization: Check if already upper to avoid alloc
        if (!IsUpperSnakeCase(name))
        {
            yield return new StringMemberPath(new[] { name.ToUpperSnakeCase() });
        }

        // 2. Permutations on Upper (normalization)
        // If name is "MY_VALUE", we permute. If name is "MyValue", ToUpperInv does work.
        foreach (var candidate in GenerateSnakeCasePermutations(name.ToUpperInvariant()))
        {
            yield return candidate;
        }

        // 3. Pascal Fallback
        var pascalName = name.ToPascalCase();
        if (string.Equals(pascalName, name, StringComparison.Ordinal))
        {
            yield break;
        }

        yield return new StringMemberPath(new[] { pascalName });
        foreach (var box in BuildCaseSensitivePermutations(pascalName))
        {
            yield return box;
        }
    }

    private static IEnumerable<StringMemberPath> GenerateSnakeCasePermutations(string name)
    {
        // Quick check before allocating array
        if (name.IndexOf('_') == -1)
        {
            yield break;
        }

        var indices = new int[MaxIndices];
        var indicesCount = FillCharIndices(name, '_', indices);

        if (indicesCount == 0)
        {
            yield break;
        }

        var permutationsCount = 1 << indicesCount;

        // Start at 1. Mask 0 means "keep all underscores", which equals the original string.
        // The original string was already yielded by the main method.
        for (var i = 1; i < permutationsCount; i++)
        {
            yield return new StringMemberPath(BuildPermutationList(name, indices, indicesCount, i, skipDelimiter: true));
        }
    }

    /// <summary>
    /// Simple check to avoid string allocation in ToSnakeCase if not needed
    /// </summary>
    private static bool IsSnakeCase(string str)
    {
        foreach (var t in str)
        {
            if (char.IsUpper(t))
                return false;
        }

        return true;
    }

    private static bool IsUpperSnakeCase(string str)
    {
        foreach (var t in str)
        {
            if (char.IsLower(t))
                return false;
        }

        return true;
    }

    private static int FillPascalSplitIndices(string str, int[] buffer)
    {
        var count = 0;
        // Start at 1. Splitting at index 0 is invalid for member paths
        for (var i = 1; i < str.Length && count < buffer.Length; i++)
        {
            if (char.IsUpper(str[i]))
            {
                buffer[count++] = i;
            }
        }

        return count;
    }

    private static int FillCharIndices(string str, char c, int[] buffer)
    {
        var count = 0;
        for (var i = 0; i < str.Length && count < buffer.Length; i++)
        {
            if (str[i] == c)
            {
                buffer[count++] = i;
            }
        }

        return count;
    }

    private static List<string> BuildPermutationList(string source, int[] indices, int indicesCount, int mask, bool skipDelimiter)
    {
        // Pre-calculate exact capacity to avoid internal List resizing
        // 1 base segment + 1 for every split
        var capacity = 1 + CountSetBits(mask);
        var list = new List<string>(capacity);

        var lastIndex = 0;

        for (var i = 0; i < indicesCount; i++)
        {
            // If bit i is set, this index is active as a split point
            if ((mask & (1 << i)) == 0)
            {
                continue;
            }

            var splitIndex = indices[i];

            // substring from last mark to here
            list.Add(source.Substring(lastIndex, splitIndex - lastIndex));

            // Move lastIndex.
            // If Pascal: Next segment starts AT the Capital letter.
            // If Snake: Next segment starts AFTER the Underscore (skipDelimiter = true).
            lastIndex = skipDelimiter ? splitIndex + 1 : splitIndex;
        }

        // Add the tail
        if (lastIndex < source.Length)
        {
            list.Add(source[lastIndex..]);
        }

        return list;
    }

    /// <summary>
    /// Fast bit counting (Hacker's Delight / Brian Kernighan’s Algorithm)
    /// </summary>
    private static int CountSetBits(int n)
    {
        var count = 0;
        while (n > 0)
        {
            n &= n - 1;
            count++;
        }

        return count;
    }
}
