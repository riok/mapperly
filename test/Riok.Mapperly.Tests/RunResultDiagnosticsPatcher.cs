using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

internal static class RunResultDiagnosticsPatcher
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Results>k__BackingField")]
    private static extern ref ImmutableArray<GeneratorRunResult> GetRunResultsField(GeneratorDriverRunResult result);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_lazyDiagnostics")]
    private static extern ref ImmutableArray<Diagnostic> GetLazyDiagnosticsField(GeneratorDriverRunResult result);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Diagnostics>k__BackingField")]
    private static extern ref ImmutableArray<Diagnostic> GetRunResultDiagnosticsField(ref GeneratorRunResult result);

    internal static void FilterRunResultDiagnostics(GeneratorDriverRunResult runResult, HashSet<string> ignoredIds)
    {
        // unfortunately there is no way with VerifySourceGenerator or the RunResult to filter the diagnostics easily.
        ref var results = ref GetRunResultsField(runResult);
        var newResults = ImmutableArray.CreateBuilder<GeneratorRunResult>(results.Length);
        foreach (var generatorResult in results)
        {
            var copy = generatorResult;
            ref var diags = ref GetRunResultDiagnosticsField(ref copy);
            diags = diags.RemoveAll(d => ignoredIds.Contains(d.Descriptor.Id));
            newResults.Add(copy);
        }

        results = newResults.ToImmutable();

        // Reset the lazy-cached aggregated diagnostics so they are recomputed from the updated results.
        GetLazyDiagnosticsField(runResult) = default;
    }
}
