using Riok.Mapperly.Configuration;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

internal static class MembersMappingStateBuilder
{
    public static MembersMappingState Build(MappingBuilderContext ctx, IMapping mapping)
    {
        // build configurations
        // duplicated configurations are filtered inside the MapValue configurations
        // if a MapProperty configuration references a target member which is already configured
        // via MapValue it is also filtered & reported
        var valueMappingConfiguredTargetPaths = new HashSet<string>();
        var configuredTargetMembersByRootName = new ListDictionary<string, IMemberPathConfiguration>();
        var memberValueConfigsByRootTargetName = BuildMemberValueConfigurations(
            ctx,
            mapping,
            valueMappingConfiguredTargetPaths,
            configuredTargetMembersByRootName
        );
        var memberConfigsByRootTargetName = BuildMemberConfigurations(
            ctx,
            mapping,
            valueMappingConfiguredTargetPaths,
            configuredTargetMembersByRootName
        );

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

        // Only expose additional parameters as source members when:
        // 1. The scope is root (this mapping declared the parameters), or
        // 2. We're in an expression context (inline within the same method scope).
        // For separate auto-generated method mappings, inherited parent parameters
        // must not shadow source type members.
        var parameterScope = ctx.ParameterScope.IsRoot || ctx.IsExpression ? ctx.ParameterScope : ParameterScope.Empty;

        return new MembersMappingState(
            unmappedSourceMemberNames,
            unmappedTargetMemberNames,
            targetMemberCaseMapping,
            targetMembers,
            memberValueConfigsByRootTargetName,
            memberConfigsByRootTargetName,
            configuredTargetMembersByRootName.AsDictionary(),
            ignoredSourceMemberNames,
            parameterScope,
            ctx.SymbolAccessor
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
        HashSet<string> configuredTargetPaths,
        ListDictionary<string, IMemberPathConfiguration> configuredTargetMembersByRootName
    )
    {
        var byTargetRootName = new ListDictionary<string, MemberValueMappingConfiguration>(ctx.Configuration.Members.ValueMappings.Count);
        foreach (var config in ctx.Configuration.Members.ValueMappings)
        {
            configuredTargetMembersByRootName.Add(config.Target.RootName, config.Target);
            if (configuredTargetPaths.Add(config.Target.FullName))
            {
                byTargetRootName.Add(config.Target.RootName, config);
                continue;
            }

            ctx.ReportDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForTargetMember,
                mapping.TargetType.ToDisplayString(),
                config.Target.FullName
            );
        }

        return byTargetRootName.AsDictionary();
    }

    private static Dictionary<string, List<MemberMappingConfiguration>> BuildMemberConfigurations(
        MappingBuilderContext ctx,
        IMapping mapping,
        HashSet<string> valueConfiguredTargetPaths,
        ListDictionary<string, IMemberPathConfiguration> configuredTargetMembersByRootName
    )
    {
        // order by target path count as objects with less path depth should be mapped first
        // to prevent NREs in the generated code
        var result = new ListDictionary<string, MemberMappingConfiguration>();
        foreach (var config in ctx.Configuration.Members.ExplicitMappings.OrderBy(cfg => cfg.Target.PathCount))
        {
            // if MapValue is already configured for this target member
            // no additional MapProperty is allowed => diagnostic duplicate config.
            if (valueConfiguredTargetPaths.Contains(config.Target.FullName))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.MultipleConfigurationsForTargetMember,
                    mapping.TargetType.ToDisplayString(),
                    config.Target.FullName
                );
                continue;
            }

            configuredTargetMembersByRootName.Add(config.Target.RootName, config.Target);
            result.Add(config.Target.RootName, config);
        }

        return result.AsDictionary();
    }
}
