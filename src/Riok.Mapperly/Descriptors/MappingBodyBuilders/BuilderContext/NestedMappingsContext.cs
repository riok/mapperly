using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// Handles nested mappings configured by <see cref="Riok.Mapperly.Abstractions.MapPropertyFromSourceAttribute"/>.
/// </summary>
public class NestedMappingsContext
{
    private readonly MappingBuilderContext _context;
    private readonly IReadOnlyCollection<MemberPath> _paths;
    private readonly IReadOnlyCollection<(MemberPath Path, ITypeSymbol RootType)> _additionalPaths;
    private readonly HashSet<MemberPath> _unusedPaths;

    private NestedMappingsContext(
        MappingBuilderContext context,
        IReadOnlyCollection<MemberPath> paths,
        IReadOnlyCollection<(MemberPath Path, ITypeSymbol RootType)> additionalPaths
    )
    {
        _context = context;
        _paths = paths;
        _additionalPaths = additionalPaths;
        _unusedPaths = new HashSet<MemberPath>(paths, ReferenceEqualityComparer.Instance);
    }

    public static NestedMappingsContext Create(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<string, IMappableMember>? additionalSourceMembers = null
    )
    {
        var paths = ResolveNestedMappings(ctx, additionalSourceMembers);
        var additionalPaths = ResolveAdditionalNestedMappings(ctx, additionalSourceMembers);
        return new NestedMappingsContext(ctx, paths, additionalPaths);
    }

    private static List<MemberPath> ResolveNestedMappings(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<string, IMappableMember>? additionalSourceMembers
    )
    {
        var nestedMemberPaths = new List<MemberPath>(ctx.Configuration.Members.NestedMappings.Count);

        foreach (var nestedMemberConfig in ctx.Configuration.Members.NestedMappings)
        {
            if (!ctx.SymbolAccessor.TryFindMemberPath(ctx.Source, nestedMemberConfig.Source, out var memberPath))
            {
                // If there are additional sources, skip the diagnostic — the path may come from one of them.
                if (additionalSourceMembers?.Values.Any(m => m.IsSpecialAdditionalSource) != true)
                {
                    ctx.ReportDiagnostic(
                        DiagnosticDescriptors.ConfiguredMappingNestedMemberNotFound,
                        nestedMemberConfig.Source.FullName,
                        ctx.Source
                    );
                }
                continue;
            }

            nestedMemberPaths.Add(memberPath);
        }

        return nestedMemberPaths;
    }

    private static List<(MemberPath Path, ITypeSymbol RootType)> ResolveAdditionalNestedMappings(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<string, IMappableMember>? additionalSourceMembers
    )
    {
        if (additionalSourceMembers == null)
            return [];

        var result = new List<(MemberPath, ITypeSymbol)>();

        foreach (var nestedMemberConfig in ctx.Configuration.Members.NestedMappings)
        {
            foreach (var (_, member) in additionalSourceMembers.Where(m => m.Value.IsSpecialAdditionalSource))
            {
                if (!ctx.SymbolAccessor.TryFindMemberPath(member.Type, nestedMemberConfig.Source, out var memberPath))
                    continue;

                result.Add((memberPath, member.Type));
                break;
            }
        }

        return result;
    }

    public bool TryFindNestedSourcePath(
        IEnumerable<StringMemberPath> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out SourceMemberPath? sourceMemberPath
    )
    {
        foreach (var nestedMemberPath in _paths)
        {
            if (
                TryFindNestedSourcePath(
                    pathCandidates,
                    ignoreCase,
                    nestedMemberPath,
                    _context.Source,
                    SourceMemberType.Member,
                    out sourceMemberPath
                )
            )
                return true;
        }

        foreach (var (path, rootType) in _additionalPaths)
        {
            if (
                TryFindNestedSourcePath(
                    pathCandidates,
                    ignoreCase,
                    path,
                    rootType,
                    SourceMemberType.AdditionalMappingMethodParameter,
                    out sourceMemberPath
                )
            )
                return true;
        }

        sourceMemberPath = default;
        return false;
    }

    private bool TryFindNestedSourcePath(
        IEnumerable<StringMemberPath> pathCandidates,
        bool ignoreCase,
        MemberPath nestedMemberPath,
        ITypeSymbol rootType,
        SourceMemberType sourceMemberType,
        [NotNullWhen(true)] out SourceMemberPath? sourceMemberPath
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
            var memberPath = new NonEmptyMemberPath(rootType, nestedMemberPath.Path.Concat(nestedSourceMemberPath.Path).ToList());
            sourceMemberPath = new SourceMemberPath(memberPath, sourceMemberType);
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
