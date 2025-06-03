using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// Builds a set of ignored source or target members
/// and reports diagnostics for any invalid configured members.
/// If a member is ignored and configured,
/// the configuration takes precedence and the member is not ignored.
/// </summary>
internal static class IgnoredMembersBuilder
{
    internal static HashSet<string> BuildIgnoredMembers(
        MappingBuilderContext ctx,
        MappingSourceTarget sourceTarget,
        IReadOnlyCollection<string> allMembers
    )
    {
        HashSet<string> ignoredMembers =
        [
            .. sourceTarget == MappingSourceTarget.Source
                ? ctx.Configuration.Members.IgnoredSources
                : ctx.Configuration.Members.IgnoredTargets,
            .. GetIgnoredAtMemberMembers(ctx, sourceTarget),
            .. GetIgnoredObsoleteMembers(ctx, sourceTarget),
        ];

        RemoveAndReportConfiguredIgnoredMembers(ctx, sourceTarget, ignoredMembers);
        ReportUnmatchedIgnoredMembers(ctx, sourceTarget, ignoredMembers, allMembers);
        return ignoredMembers;
    }

    private static void RemoveAndReportConfiguredIgnoredMembers(
        MappingBuilderContext ctx,
        MappingSourceTarget sourceTarget,
        HashSet<string> ignoredMembers
    )
    {
        var isSource = sourceTarget == MappingSourceTarget.Source;
        var diagnostic = isSource
            ? DiagnosticDescriptors.IgnoredSourceMemberExplicitlyMapped
            : DiagnosticDescriptors.IgnoredTargetMemberExplicitlyMapped;
        var type = isSource ? ctx.Source : ctx.Target;
        var configuredMembers = ctx.Configuration.Members.GetMembersWithExplicitConfigurations(sourceTarget);
        foreach (var configuredMember in configuredMembers)
        {
            if (ignoredMembers.Remove(configuredMember))
            {
                ctx.ReportDiagnostic(diagnostic, configuredMember, type);
            }
        }
    }

    private static void ReportUnmatchedIgnoredMembers(
        MappingBuilderContext ctx,
        MappingSourceTarget sourceTarget,
        IEnumerable<string> ignoredMembers,
        IEnumerable<string> allMembers
    )
    {
        var isSource = sourceTarget == MappingSourceTarget.Source;
        var type = isSource ? ctx.Source : ctx.Target;
        var nestedDiagnostic = isSource ? DiagnosticDescriptors.NestedIgnoredSourceMember : DiagnosticDescriptors.NestedIgnoredTargetMember;
        var notFoundDiagnostic = isSource
            ? DiagnosticDescriptors.IgnoredSourceMemberNotFound
            : DiagnosticDescriptors.IgnoredTargetMemberNotFound;

        var unmatchedMembers = new HashSet<string>(ignoredMembers);
        unmatchedMembers.ExceptWith(allMembers);

        foreach (var member in unmatchedMembers)
        {
            if (member.Contains(MemberPathConstants.MemberAccessSeparator, StringComparison.Ordinal))
            {
                ctx.ReportDiagnostic(nestedDiagnostic, member, type);
                continue;
            }

            ctx.ReportDiagnostic(notFoundDiagnostic, member, type);
        }
    }

    private static IEnumerable<string> GetIgnoredAtMemberMembers(MappingBuilderContext ctx, MappingSourceTarget sourceTarget)
    {
        var type = sourceTarget == MappingSourceTarget.Source ? ctx.Source : ctx.Target;

        return ctx.SymbolAccessor.GetAllAccessibleMappableMembers(type).Where(x => x.IsIgnored).Select(x => x.Name);
    }

    private static IEnumerable<string> GetIgnoredObsoleteMembers(MappingBuilderContext ctx, MappingSourceTarget sourceTarget)
    {
        var obsoleteStrategy = ctx.GetIgnoreObsoleteMembersStrategy();
        var strategy =
            sourceTarget == MappingSourceTarget.Source ? IgnoreObsoleteMembersStrategy.Source : IgnoreObsoleteMembersStrategy.Target;

        if (!obsoleteStrategy.HasFlag(strategy))
            return [];

        var type = sourceTarget == MappingSourceTarget.Source ? ctx.Source : ctx.Target;

        return ctx.SymbolAccessor.GetAllAccessibleMappableMembers(type).Where(x => x.IsObsolete).Select(x => x.Name);
    }
}
