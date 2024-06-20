using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// Handles nested mappings configured by <see cref="Riok.Mapperly.Abstractions.MapPropertyFromSourceAttribute"/>.
/// </summary>
public class NestedMappingsContext
{
    private readonly MappingBuilderContext _context;
    private readonly IReadOnlyCollection<MemberPath> _paths;
    private readonly HashSet<MemberPath> _unusedPaths;

    private NestedMappingsContext(MappingBuilderContext context, IReadOnlyCollection<MemberPath> paths)
    {
        _context = context;
        _paths = paths;
        _unusedPaths = new HashSet<MemberPath>(paths, ReferenceEqualityComparer.Instance);
    }

    public static NestedMappingsContext Create(MappingBuilderContext ctx) => new(ctx, ResolveNestedMappings(ctx));

    private static List<MemberPath> ResolveNestedMappings(MappingBuilderContext ctx)
    {
        var nestedMemberPaths = new List<MemberPath>(ctx.Configuration.Members.NestedMappings.Count);

        foreach (var nestedMemberConfig in ctx.Configuration.Members.NestedMappings)
        {
            if (!ctx.SymbolAccessor.TryFindMemberPath(ctx.Source, nestedMemberConfig.Source.Path, out var memberPath))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.ConfiguredMappingNestedMemberNotFound,
                    nestedMemberConfig.Source.FullName,
                    ctx.Source
                );
                continue;
            }

            nestedMemberPaths.Add(memberPath);
        }

        return nestedMemberPaths;
    }

    public bool TryFindNestedSourcePath(
        List<List<string>> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? sourceMemberPath
    )
    {
        foreach (var nestedMemberPath in _paths)
        {
            if (TryFindNestedSourcePath(pathCandidates, ignoreCase, nestedMemberPath, out sourceMemberPath))
                return true;
        }

        sourceMemberPath = default;
        return false;
    }

    private bool TryFindNestedSourcePath(
        List<List<string>> pathCandidates,
        bool ignoreCase,
        MemberPath nestedMemberPath,
        [NotNullWhen(true)] out MemberPath? sourceMemberPath
    )
    {
        if (
            _context.SymbolAccessor.TryFindMemberPath(
                nestedMemberPath.MemberType,
                pathCandidates,
                // Use empty ignore list to support ignoring a property for normal search while flattening its properties
                Array.Empty<string>(),
                ignoreCase,
                out var nestedSourceMemberPath
            )
        )
        {
            sourceMemberPath = new NonEmptyMemberPath(_context.Source, nestedMemberPath.Path.Concat(nestedSourceMemberPath.Path).ToList());
            _unusedPaths.Remove(nestedMemberPath);
            return true;
        }

        sourceMemberPath = default;
        return false;
    }

    public void ReportDiagnostics()
    {
        foreach (var unusedPath in _unusedPaths)
        {
            _context.ReportDiagnostic(
                DiagnosticDescriptors.NestedMemberNotUsed,
                unusedPath.ToDisplayString(includeMemberType: false, includeRootType: false),
                _context.Source.ToDisplayString()
            );
        }
    }
}
