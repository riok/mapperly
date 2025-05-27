using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

public class IncludeMappingBuilder(MappingCollection mappings)
{
    private readonly Dictionary<IMapping, MappingBuilderContext> _visited = [];
    private readonly HashSet<IMapping> _processing = [];

    public void Build(CancellationToken token)
    {
        var mappingsCopy = mappings.DequeueMappingsToBuildBody().ToList();
        foreach (var (mapping, ctx) in mappingsCopy)
        {
            var result = BuildRecursively(mapping, ctx, token);
            mappings.EnqueueToBuildBody(mapping, result);
        }
    }

    private MappingBuilderContext BuildRecursively(IMapping mapping, MappingBuilderContext ctx, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (_visited.TryGetValue(mapping, out var existingCtx))
        {
            return existingCtx;
        }

        if (ctx.GetType() != typeof(MappingBuilderContext))
        {
            // Not supported yet.
            return ctx;
        }

        if (!_processing.Add(mapping))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CircularReferencedMapping, ctx.UserSymbol?.ToDisplayString() ?? "<unknown>");
            return ctx;
        }

        try
        {
            if (!TryResolveIncludedMapping(ctx, out var includedMapping, out var includedMappingContext))
            {
                _visited[mapping] = ctx;
                return ctx;
            }

            includedMappingContext = BuildRecursively(includedMapping, includedMappingContext, token);
            var newCtx = ctx.IncludeMappingBuilderContext(includedMappingContext);

            _visited[mapping] = newCtx;
            return newCtx;
        }
        finally
        {
            _processing.Remove(mapping);
        }
    }

    /// <summary>
    /// Attempts to resolve an included mapping and its associated context. If multiple mappings are found,
    /// then tries to find a single match that maps the same source and target types or base types.
    /// </summary>
    /// <param name="ctx">The source context where the mapping resolution is attended.</param>
    /// <param name="includedMapping">The resolved included mapping if successful, or <c>null</c> otherwise.</param>
    /// <param name="includedMappingContext">The resolved mapping context if successful, or <c>null</c> otherwise.</param>
    /// <returns><c>true</c> if an included mapping and context are successfully resolved; otherwise, <c>false</c>.</returns>
    public bool TryResolveIncludedMapping(
        MappingBuilderContext ctx,
        [NotNullWhen(true)] out ITypeMapping? includedMapping,
        [NotNullWhen(true)] out MappingBuilderContext? includedMappingContext
    )
    {
        var mappingName = ctx.Configuration.Members.IncludedMapping;
        if (mappingName == null)
        {
            includedMapping = null;
            includedMappingContext = null;
            return false;
        }

        var contexts = mappings.FindMappingBuilderContext(mappingName);
        switch (contexts.Count)
        {
            case 0:
                includedMapping = null;
                includedMappingContext = null;
                ctx.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, mappingName);
                return false;
            case 1:
            {
                includedMappingContext = contexts[0];
                includedMapping = includedMappingContext.UserMapping!;
                var checkerResult = CheckIncludedMappingTypes(ctx, includedMappingContext);
                return ReportFailedIncludeMapping(ctx, checkerResult, includedMappingContext);
            }
            default:
                return TryGetBestIncludedMappingCandidate(ctx, out includedMapping, out includedMappingContext, contexts, mappingName);
        }
    }

    private bool TryGetBestIncludedMappingCandidate(
        MappingBuilderContext ctx,
        out ITypeMapping? includedMapping,
        out MappingBuilderContext? includedMappingContext,
        IReadOnlyList<MappingBuilderContext> contexts,
        string mappingName
    )
    {
        includedMapping = null;
        includedMappingContext = null;
        var candidates = new List<MappingBuilderContext>();
        foreach (var childContextCandidate in contexts)
        {
            var checkerResult = CheckIncludedMappingTypes(ctx, childContextCandidate);
            if (checkerResult.Success)
            {
                candidates.Add(childContextCandidate);
            }
        }

        switch (candidates.Count)
        {
            case 0:
                ctx.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, mappingName);
                return false;
            case 1:
                includedMappingContext = candidates[0];
                includedMapping = includedMappingContext.UserMapping!;
                return true;
            default:
                ctx.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingAmbiguous, mappingName);
                return false;
        }
    }

    private GenericTypeChecker.GenericTypeCheckerResult CheckIncludedMappingTypes(
        MappingBuilderContext ctx,
        MappingBuilderContext includedMapping
    )
    {
        return ctx.GenericTypeChecker.InferAndCheckTypes(
            ctx.UserSymbol!.TypeParameters,
            (includedMapping.Source, ctx.Source),
            (includedMapping.Target, ctx.Target)
        );
    }

    private bool ReportFailedIncludeMapping(
        MappingBuilderContext ctx,
        GenericTypeChecker.GenericTypeCheckerResult typeCheckerResult,
        MappingBuilderContext includedMapping
    )
    {
        if (typeCheckerResult.Success)
        {
            return true;
        }

        if (ReferenceEquals(ctx.Source, typeCheckerResult.FailedArgument))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.SourceTypeIsNotAssignableToTheIncludedSourceType,
                ctx.Source,
                includedMapping.Source
            );
        }
        else
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.TargetTypeIsNotAssignableToTheIncludedTargetType,
                ctx.Target,
                includedMapping.Target
            );
        }

        return false;
    }
}
