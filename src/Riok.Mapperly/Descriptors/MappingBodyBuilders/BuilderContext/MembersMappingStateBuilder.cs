using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

internal static class MembersMappingStateBuilder
{
    public static MembersMappingState Build(MappingBuilderContext ctx, IMapping mapping)
    {
        // build configurations
        var configuredTargetMembers = new HashSet<StringMemberPath>();
        var memberValueConfigsByRootTargetName = BuildMemberValueConfigurations(ctx, mapping, configuredTargetMembers);
        var memberConfigsByRootTargetName = BuildMemberConfigurations(ctx, mapping, configuredTargetMembers);

        // build all members
        var unmappedSourceMemberNames = GetSourceMemberNames(ctx, mapping);
        var targetMembers = GetTargetMembers(ctx, mapping);

        // build ignored members
        var ignoredSourceMemberNames = IgnoredMembersBuilder.BuildIgnoredMembers(
            ctx,
            MappingSourceTarget.Source,
            unmappedSourceMemberNames
        );
        var ignoredTargetMemberNames = IgnoredMembersBuilder.BuildIgnoredMembers(ctx, MappingSourceTarget.Target, targetMembers.Keys);

        // remove ignored members
        unmappedSourceMemberNames.ExceptWith(ignoredSourceMemberNames);
        targetMembers.RemoveRange(ignoredTargetMemberNames);

        var targetMemberCaseMapping = targetMembers
            .Keys.GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var unmappedTargetMemberNames = targetMembers.Keys.ToHashSet();
        return new MembersMappingState(
            unmappedSourceMemberNames,
            unmappedTargetMemberNames,
            targetMemberCaseMapping,
            targetMembers,
            memberValueConfigsByRootTargetName,
            memberConfigsByRootTargetName,
            ignoredSourceMemberNames
        );
    }

    private static HashSet<string> GetSourceMemberNames(MappingBuilderContext ctx, IMapping mapping)
    {
        return ctx.SymbolAccessor.GetAllAccessibleMappableMembers(mapping.SourceType).Select(x => x.Name).ToHashSet();
    }

    private static Dictionary<string, IMappableMember> GetTargetMembers(MappingBuilderContext ctx, IMapping mapping)
    {
        return ctx.SymbolAccessor.GetAllAccessibleMappableMembers(mapping.TargetType).ToDictionary(x => x.Name);
    }

    private static Dictionary<string, List<MemberValueMappingConfiguration>> BuildMemberValueConfigurations(
        MappingBuilderContext ctx,
        IMapping mapping,
        HashSet<StringMemberPath> configuredTargetMembers
    )
    {
        return GetUniqueTargetConfigurations(ctx, mapping, configuredTargetMembers, ctx.Configuration.Members.ValueMappings, x => x.Target)
            .GroupBy(x => x.Target.Path[0])
            .ToDictionary(x => x.Key, x => x.ToList());
    }

    private static Dictionary<string, List<MemberMappingConfiguration>> BuildMemberConfigurations(
        MappingBuilderContext ctx,
        IMapping mapping,
        HashSet<StringMemberPath> configuredTargetMembers
    )
    {
        // order by target path count as objects with less path depth should be mapped first
        // to prevent NREs in the generated code
        return GetUniqueTargetConfigurations(
                ctx,
                mapping,
                configuredTargetMembers,
                ctx.Configuration.Members.ExplicitMappings,
                x => x.Target
            )
            .GroupBy(x => x.Target.Path[0])
            .ToDictionary(x => x.Key, x => x.OrderBy(cfg => cfg.Target.Path.Count).ToList());
    }

    private static IEnumerable<T> GetUniqueTargetConfigurations<T>(
        MappingBuilderContext ctx,
        IMapping mapping,
        HashSet<StringMemberPath> configuredTargetMembers,
        IEnumerable<T> configs,
        Func<T, StringMemberPath> targetPathSelector
    )
    {
        foreach (var config in configs)
        {
            var targetPath = targetPathSelector(config);
            if (configuredTargetMembers.Add(targetPath))
            {
                yield return config;
                continue;
            }

            ctx.ReportDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForTargetMember,
                mapping.TargetType.ToDisplayString(),
                targetPath.FullName
            );
        }
    }
}
