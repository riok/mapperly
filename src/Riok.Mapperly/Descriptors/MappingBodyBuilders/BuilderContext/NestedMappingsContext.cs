using System.Diagnostics.CodeAnalysis;
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
    private readonly IReadOnlyCollection<MemberPath> _additionalPaths;
    private readonly HashSet<MemberPath> _unusedPaths;

    private NestedMappingsContext(
        MappingBuilderContext context,
        IReadOnlyCollection<MemberPath> paths,
        IReadOnlyCollection<MemberPath> additionalPaths
    )
    {
        _context = context;
        _paths = paths;
        _additionalPaths = additionalPaths;
        _unusedPaths = new HashSet<MemberPath>(paths, ReferenceEqualityComparer.Instance);
    }

    public static NestedMappingsContext Create(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<string, IMappableMember> additionalSourceMembers
    ) => new(ctx, ResolveNestedMappings(ctx, additionalSourceMembers), ResolveAddNestedMappings(ctx, additionalSourceMembers));

    private static IReadOnlyCollection<MemberPath> ResolveNestedMappings(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<string, IMappableMember> additionalSourceMembers
    )
    {
        var nestedMemberPaths = new List<MemberPath>(ctx.Configuration.Members.NestedMappings.Count);

        foreach (var nestedMemberConfig in ctx.Configuration.Members.NestedMappings)
        {
            var source = ctx.Source;
            if (!ctx.SymbolAccessor.TryFindMemberPath(source, nestedMemberConfig.Source, out var memberPath))
            {
                var any = additionalSourceMembers.Any(m => m.Value.IsSpecialAdditionalSource);
                if (!any)
                {
                    ctx.ReportDiagnostic(
                        DiagnosticDescriptors.ConfiguredMappingNestedMemberNotFound,
                        nestedMemberConfig.Source.FullName,
                        source
                    );
                    continue;
                }
            }

            if (memberPath is not null)
                nestedMemberPaths.Add(memberPath);
        }

        return nestedMemberPaths;
    }

    // TODO: What if source already have the same nested, maybe as prior get nested from source
    // TODO: It is just copy of ResolveNestedMappings
    private static IReadOnlyCollection<MemberPath> ResolveAddNestedMappings(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<string, IMappableMember> additionalSourceMembers
    )
    {
        var nestedMemberPaths = new List<MemberPath>(ctx.Configuration.Members.NestedMappings.Count);
        foreach (var nestedMemberConfig in ctx.Configuration.Members.NestedMappings)
        {
            var classes = additionalSourceMembers.Where(m => m.Value.IsSpecialAdditionalSource).ToList();
            foreach (var (_, value) in classes)
            {
                if (!ctx.SymbolAccessor.TryFindMemberPath(value.Type, nestedMemberConfig.Source, out var memberPath))
                {
                    continue;
                }

                nestedMemberPaths.Add(memberPath);
                break;
            }

            // TODO: Do something with report diagnostic
        }

        return nestedMemberPaths;
    }

    public bool TryFindNestedSourcePath(
        IReadOnlyCollection<StringMemberPath> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out SourceMemberPath? sourceMemberPath
    )
    {
        foreach (var nestedMemberPath in _paths)
        {
            if (TryFindNestedSourcePath(pathCandidates, ignoreCase, nestedMemberPath, SourceMemberType.Member, out sourceMemberPath))
                return true;
        }

        foreach (var nestedMemberPath in _additionalPaths)
        {
            if (
                TryFindNestedSourcePath(
                    pathCandidates,
                    ignoreCase,
                    nestedMemberPath,
                    SourceMemberType.AdditionalMappingMethodParameter,
                    out sourceMemberPath
                )
            )
                return true;
        }

        sourceMemberPath = null;
        return false;
    }

    private bool TryFindNestedSourcePath(
        IEnumerable<StringMemberPath> pathCandidates,
        bool ignoreCase,
        MemberPath nestedMemberPath,
        SourceMemberType sourceMemberType,
        [NotNullWhen(true)] out SourceMemberPath? sourceMemberPath
    )
    {
        if (
            _context.SymbolAccessor.TryFindMemberPath(
                nestedMemberPath.MemberType,
                pathCandidates,
                // Use empty ignore list to support ignoring a property for normal search while flattening its properties
                [],
                ignoreCase,
                out var nestedSourceMemberPath
            )
        )
        {
            var memberPath = new NonEmptyMemberPath(
                nestedMemberPath.RootType,
                nestedMemberPath.Path.Concat(nestedSourceMemberPath.Path).ToList()
            );
            sourceMemberPath = new SourceMemberPath(memberPath, sourceMemberType);
            _unusedPaths.Remove(nestedMemberPath);
            return true;
        }

        sourceMemberPath = null;
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
