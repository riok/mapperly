using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

internal class MembersMappingStateBuilder
{
    private readonly HashSet<IMapping> _mappingsInProcessing = new();
    private readonly HashSet<string> _valueMappingConfiguredTargetPaths = [];
    private readonly ListDictionary<string, IMemberPathConfiguration> _configuredTargetMembersByRootName = new();

    private readonly Dictionary<string, IMappableMember> _additionalSourceMembers = new(StringComparer.OrdinalIgnoreCase);

    private readonly ListDictionary<string, MemberValueMappingConfiguration> _memberValueConfigsByRootTargetName;
    private readonly ListDictionary<string, MemberMappingConfiguration> _memberConfigsByRootTargetName = new();
    private readonly HashSet<string> _ignoredSourceMemberNames = new();
    private readonly HashSet<string> _ignoredTargetMemberNames = new();

    private MembersMappingStateBuilder(MappingBuilderContext ctx)
    {
        var initialCapacity = ctx.Configuration.Members.ValueMappings.Count;
        _memberValueConfigsByRootTargetName = new ListDictionary<string, MemberValueMappingConfiguration>(initialCapacity);
    }

    public static MembersMappingState Build(MappingBuilderContext ctx, IMapping mapping)
    {
        var factory = new MembersMappingStateBuilder(ctx);

        // build all members
        var unmappedSourceMemberNames = GetSourceMemberNames(ctx, mapping);
        var targetMembers = GetTargetMembers(ctx, mapping);

        factory.BuildRecursively(ctx, mapping);

        // after collecting the ignored members cle
        IgnoredMembersBuilder.CleanupIgnoredMembers(
            factory._ignoredSourceMemberNames,
            ctx,
            MappingSourceTarget.Source,
            unmappedSourceMemberNames
        );

        IgnoredMembersBuilder.CleanupIgnoredMembers(factory._ignoredTargetMemberNames, ctx, MappingSourceTarget.Target, targetMembers.Keys);

        // remove ignored members
        unmappedSourceMemberNames.ExceptWith(factory._ignoredSourceMemberNames);
        targetMembers.RemoveRange(factory._ignoredTargetMemberNames);

        var targetMemberCaseMapping = targetMembers
            .Keys.GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var unmappedTargetMemberNames = targetMembers.Keys.ToHashSet();

        var unmappedAdditionalSourceMemberNames = new HashSet<string>(factory._additionalSourceMembers.Keys, StringComparer.Ordinal);
        return new MembersMappingState(
            unmappedSourceMemberNames,
            unmappedAdditionalSourceMemberNames,
            unmappedTargetMemberNames,
            factory._additionalSourceMembers,
            targetMemberCaseMapping,
            targetMembers,
            factory._memberValueConfigsByRootTargetName.AsDictionary(),
            factory._memberConfigsByRootTargetName.AsDictionary(),
            factory._configuredTargetMembersByRootName.AsDictionary(),
            factory._ignoredSourceMemberNames
        );
    }

    private void BuildRecursively(MappingBuilderContext ctx, IMapping mapping)
    {
        if (!_mappingsInProcessing.Add(mapping))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.CircularReferencedMapping, ctx.UserSymbol?.ToDisplayString() ?? "<unknown>");
            return;
        }

        // first collect the mappings from the included mappings, then apply the mappings from the current mapping.
        if (TryGetIncludedMapping(ctx, out var includedMapping, out var includedMappingContext))
        {
            BuildRecursively(includedMappingContext, includedMapping);
        }

        // build configurations
        // duplicated configurations are filtered inside the MapValue configurations
        // if a MapProperty configuration references a target member which is already configured
        // via MapValue, it is also filtered and reported
        BuildMemberValueConfigurations(ctx, mapping);
        BuildMemberConfigurations(ctx, mapping);
        GetAdditionalSourceMembers(ctx);

        // build ignored members
        IgnoredMembersBuilder.CollectIgnoredMembers(_ignoredSourceMemberNames, ctx, MappingSourceTarget.Source);
        IgnoredMembersBuilder.CollectIgnoredMembers(_ignoredTargetMemberNames, ctx, MappingSourceTarget.Target);

        _mappingsInProcessing.Remove(mapping);
    }

    private void GetAdditionalSourceMembers(MappingBuilderContext ctx)
    {
        if (ctx.UserMapping is MethodMapping { AdditionalSourceParameters.Count: > 0 } methodMapping)
        {
            foreach (
                var mappableMember in methodMapping.AdditionalSourceParameters.Select<MethodParameter, IMappableMember>(
                    p => new ParameterSourceMember(p)
                )
            )
            {
                _additionalSourceMembers.Add(mappableMember.Name, mappableMember);
            }
        }
    }

    private static HashSet<string> GetSourceMemberNames(MappingBuilderContext ctx, IMapping mapping)
    {
        return ctx.SymbolAccessor.GetAllAccessibleMappableMembers(mapping.SourceType).Select(x => x.Name).ToHashSet();
    }

    private static Dictionary<string, IMappableMember> GetTargetMembers(MappingBuilderContext ctx, IMapping mapping)
    {
        return ctx.SymbolAccessor.GetAllAccessibleMappableMembers(mapping.TargetType).ToDictionary(x => x.Name);
    }

    private void BuildMemberValueConfigurations(MappingBuilderContext ctx, IMapping mapping)
    {
        foreach (var config in ctx.Configuration.Members.ValueMappings)
        {
            _configuredTargetMembersByRootName.Add(config.Target.RootName, config.Target);
            if (_valueMappingConfiguredTargetPaths.Add(config.Target.FullName))
            {
                _memberValueConfigsByRootTargetName.Add(config.Target.RootName, config);
                continue;
            }

            ctx.ReportDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForTargetMember,
                mapping.TargetType.ToDisplayString(),
                config.Target.FullName
            );
        }
    }

    private void BuildMemberConfigurations(MappingBuilderContext ctx, IMapping mapping)
    {
        // order by target path count as objects with less path depth should be mapped first
        // to prevent NREs in the generated code
        foreach (var config in ctx.Configuration.Members.ExplicitMappings.OrderBy(cfg => cfg.Target.PathCount))
        {
            // if MapValue is already configured for this target member,
            // no additional MapProperty is allowed => diagnostic duplicate config.
            if (_valueMappingConfiguredTargetPaths.Contains(config.Target.FullName))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.MultipleConfigurationsForTargetMember,
                    mapping.TargetType.ToDisplayString(),
                    config.Target.FullName
                );
                continue;
            }

            _configuredTargetMembersByRootName.Add(config.Target.RootName, config.Target);
            _memberConfigsByRootTargetName.Add(config.Target.RootName, config);
        }
    }

    private static bool TryGetIncludedMapping(
        MappingBuilderContext ctx,
        [NotNullWhen(true)] out INewInstanceMapping? includedMapping,
        [NotNullWhen(true)] out MappingBuilderContext? includedMappingContext
    )
    {
        includedMapping = null;
        includedMappingContext = null;
        var includedMappingName = ctx.Configuration.Members.IncludedMapping;
        if (includedMappingName == null)
        {
            return false;
        }

        includedMapping = ctx.FindNamedMapping(includedMappingName);
        if (includedMapping == null)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.ReferencedMappingNotFound, includedMappingName);
            return false;
        }

        var typeCheckerResult = ctx.GenericTypeChecker.InferAndCheckTypes(
            ctx.UserSymbol!.TypeParameters,
            (includedMapping.SourceType, ctx.Source),
            (includedMapping.TargetType, ctx.Target)
        );
        if (!typeCheckerResult.Success)
        {
            if (ReferenceEquals(ctx.Source, typeCheckerResult.FailedArgument))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.SourceTypeIsNotRelatedToIncludedSourceType,
                    ctx.Source,
                    includedMapping.SourceType
                );
            }
            else
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.TargetTypeIsNotRelatedToIncludedTargetType,
                    ctx.Target,
                    includedMapping.TargetType
                );
            }
        }
        else
        {
            includedMappingContext = ctx.FindMappingBuilderContext(includedMapping);
            return includedMappingContext != null;
        }

        return false;
    }
}
