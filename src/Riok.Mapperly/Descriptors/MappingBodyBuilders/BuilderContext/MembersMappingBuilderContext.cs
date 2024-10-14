using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An abstract base implementation of <see cref="IMembersBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public abstract class MembersMappingBuilderContext<T>(MappingBuilderContext builderContext, T mapping) : IMembersBuilderContext<T>
    where T : IMapping
{
    private readonly MembersMappingState _state = MembersMappingStateBuilder.Build(builderContext, mapping);

    private readonly NestedMappingsContext _nestedMappingsContext = NestedMappingsContext.Create(builderContext);

    public MappingBuilderContext BuilderContext { get; } = builderContext;

    public T Mapping { get; } = mapping;

    public void AddDiagnostics()
    {
        MemberMappingDiagnosticReporter.ReportDiagnostics(BuilderContext, _state);
        _nestedMappingsContext.ReportDiagnostics();
    }

    public IEnumerable<IMappableMember> EnumerateUnmappedTargetMembers() => _state.EnumerateUnmappedTargetMembers();

    public IEnumerable<IMappableMember> EnumerateUnmappedOrConfiguredTargetMembers() => _state.EnumerateUnmappedOrConfiguredTargetMembers();

    public void TryAddSourceMemberAlias(string alias, IMappableMember member) => _state.TryAddSourceMemberAlias(alias, member);

    public void SetTargetMemberMapped(IMappableMember targetMember) => _state.SetTargetMemberMapped(targetMember);

    protected void SetTargetMemberMapped(string targetMemberName, bool ignoreCase = false) =>
        _state.SetTargetMemberMapped(targetMemberName, ignoreCase);

    public void SetMembersMapped(MemberMappingInfo members) => _state.SetMembersMapped(members, false);

    public void IgnoreMembers(IMappableMember member) => _state.IgnoreMembers(member);

    public void IgnoreMembers(string memberName) => _state.IgnoreMembers(memberName);

    public void ConsumeMemberConfigs(MemberMappingInfo members)
    {
        if (members.Configuration != null)
        {
            ConsumeMemberConfig(members.Configuration);
        }

        if (members.ValueConfiguration != null)
        {
            ConsumeMemberConfig(members.ValueConfiguration);
        }
    }

    public void MappingAdded() => _state.MappingAdded();

    protected void MappingAdded(MemberMappingInfo info, bool ignoreTargetCasing = false) => _state.MappingAdded(info, ignoreTargetCasing);

    protected void ConsumeMemberConfig(MemberMappingConfiguration config) => _state.ConsumeMemberConfig(config);

    protected void ConsumeMemberConfig(MemberValueMappingConfiguration config) => _state.ConsumeMemberConfig(config);

    public IEnumerable<MemberMappingInfo> MatchMembers(IMappableMember targetMember)
    {
        var matchedMemberMappingInfos = new List<MemberMappingInfo>();
        if (TryGetMemberValueConfigs(targetMember.Name, false, out var memberValueConfigs))
        {
            matchedMemberMappingInfos.AddRange(ResolveMemberMappingInfo(memberValueConfigs.ToList()));
        }

        if (TryGetMemberConfigs(targetMember.Name, false, out var memberConfigs))
        {
            matchedMemberMappingInfos.AddRange(ResolveMemberMappingInfo(memberConfigs.ToList()));
        }

        if (matchedMemberMappingInfos.Count > 0)
        {
            // return configs with shorter target member paths first
            // to prevent NRE's in the generated code
            return matchedMemberMappingInfos.OrderBy(x => x.TargetMember.Path.Count);
        }

        // match directly
        if (TryFindSourcePath(targetMember.Name, out var sourceMemberPath))
        {
            return [new MemberMappingInfo(sourceMemberPath, new NonEmptyMemberPath(Mapping.TargetType, [targetMember]))];
        }

        return [];
    }

    protected bool TryMatchMember(IMappableMember targetMember, [NotNullWhen(true)] out MemberMappingInfo? memberInfo) =>
        TryMatchMember(targetMember, null, out memberInfo);

    protected bool TryMatchMember(IMappableMember targetMember, bool? ignoreCase, [NotNullWhen(true)] out MemberMappingInfo? memberInfo)
    {
        if (TryGetConfiguredMemberMappingInfo(targetMember, ignoreCase == true, out memberInfo))
            return true;

        // if no config was found, match the source path
        if (TryFindSourcePath(targetMember.Name, out var sourceMemberPath, ignoreCase))
        {
            memberInfo = new MemberMappingInfo(sourceMemberPath, new NonEmptyMemberPath(BuilderContext.Target, [targetMember]));
            return true;
        }

        memberInfo = null;
        return false;
    }

    protected bool TryGetMemberConfigs(
        string targetMemberName,
        bool ignoreCase,
        [NotNullWhen(true)] out IReadOnlyList<MemberMappingConfiguration>? memberConfigs
    ) => _state.TryGetMemberConfigs(targetMemberName, ignoreCase, out memberConfigs);

    protected bool TryGetMemberValueConfigs(
        string targetMemberName,
        bool ignoreCase,
        [NotNullWhen(true)] out IReadOnlyList<MemberValueMappingConfiguration>? memberConfigs
    ) => _state.TryGetMemberValueConfigs(targetMemberName, ignoreCase, out memberConfigs);

    protected virtual bool TryFindSourcePath(
        IEnumerable<StringMemberPath> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out SourceMemberPath? sourcePath
    )
    {
        // try to match in additional source members
        if (
            BuilderContext.SymbolAccessor.TryFindMemberPath(
                _state.AdditionalSourceMembers,
                pathCandidates,
                ignoreCase,
                out var sourceMemberPath
            )
        )
        {
            sourcePath = new SourceMemberPath(sourceMemberPath, SourceMemberType.AdditionalMappingMethodParameter);
            return true;
        }

        // try to match in aliased source members
        if (BuilderContext.SymbolAccessor.TryFindMemberPath(_state.AliasedSourceMembers, pathCandidates, ignoreCase, out sourceMemberPath))
        {
            sourcePath = new SourceMemberPath(sourceMemberPath, SourceMemberType.MemberAlias);
            return true;
        }

        // match against source type members
        if (
            BuilderContext.SymbolAccessor.TryFindMemberPath(
                Mapping.SourceType,
                pathCandidates,
                _state.IgnoredSourceMemberNames,
                ignoreCase,
                out sourceMemberPath
            )
        )
        {
            sourcePath = new SourceMemberPath(sourceMemberPath, SourceMemberType.Member);
            return true;
        }

        sourcePath = null;
        return false;
    }

    protected bool IsIgnoredSourceMember(string sourceMemberName) => _state.IgnoredSourceMemberNames.Contains(sourceMemberName);

    private IReadOnlyList<MemberMappingInfo> ResolveMemberMappingInfo(IEnumerable<MemberMappingConfiguration> configs) =>
        configs.Select(ResolveMemberMappingInfo).WhereNotNull().ToList();

    private IEnumerable<MemberMappingInfo> ResolveMemberMappingInfo(IEnumerable<MemberValueMappingConfiguration> configs) =>
        configs.Select(ResolveMemberMappingInfo).WhereNotNull().ToList();

    private MemberMappingInfo? ResolveMemberMappingInfo(MemberValueMappingConfiguration config)
    {
        if (
            !BuilderContext.SymbolAccessor.TryFindMemberPath(Mapping.TargetType, config.Target, out var foundMemberPath)
            || foundMemberPath is not NonEmptyMemberPath targetMemberPath
        )
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound,
                config.Target.FullName,
                Mapping.TargetType
            );

            // consume this member config and prevent its further usage
            // as it is invalid, and a diagnostic has already been reported
            _state.ConsumeMemberConfig(config);
            return null;
        }

        return new MemberMappingInfo(targetMemberPath, config);
    }

    private MemberMappingInfo? ResolveMemberMappingInfo(MemberMappingConfiguration config)
    {
        if (
            !BuilderContext.SymbolAccessor.TryFindMemberPath(Mapping.TargetType, config.Target, out var foundMemberPath)
            || foundMemberPath is not NonEmptyMemberPath targetMemberPath
        )
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound,
                config.Target.FullName,
                Mapping.TargetType
            );

            // consume this member config and prevent its further usage
            // as it is invalid, and a diagnostic has already been reported
            _state.ConsumeMemberConfig(config);
            return null;
        }

        if (!ResolveMemberConfigSourcePath(config, out var sourceMemberPath))
            return null;

        return new MemberMappingInfo(sourceMemberPath, targetMemberPath, config);
    }

    private bool ResolveMemberConfigSourcePath(MemberMappingConfiguration config, [NotNullWhen(true)] out SourceMemberPath? sourcePath)
    {
        if (!BuilderContext.SymbolAccessor.TryFindMemberPath(Mapping.SourceType, config.Source, out var sourceMemberPath))
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingSourceMemberNotFound,
                config.Source.FullName,
                Mapping.SourceType
            );

            // consume this member config and prevent its further usage
            // as it is invalid, and a diagnostic has already been reported
            _state.ConsumeMemberConfig(config);
            sourcePath = null;
            return false;
        }

        sourcePath = new SourceMemberPath(sourceMemberPath, SourceMemberType.Member);
        return true;
    }

    private bool TryFindSourcePath(
        string targetMemberName,
        [NotNullWhen(true)] out SourceMemberPath? sourceMemberPath,
        bool? ignoreCase = null
    )
    {
        ignoreCase ??= BuilderContext.Configuration.Mapper.PropertyNameMappingStrategy == PropertyNameMappingStrategy.CaseInsensitive;
        var pathCandidates = MemberPathCandidateBuilder.BuildMemberPathCandidates(targetMemberName);

        // First, try to find the property on (a sub-path of) the source type itself. (If this is undesired, an Ignore property can be used.)
        if (TryFindSourcePath(pathCandidates, ignoreCase.Value, out sourceMemberPath))
            return true;

        // Otherwise, search all nested members
        return _nestedMappingsContext.TryFindNestedSourcePath(pathCandidates, ignoreCase.Value, out sourceMemberPath);
    }

    private MemberMappingInfo? TryGetMemberConfigMappingInfo(IMappableMember targetMember, bool ignoreCase)
    {
        if (TryGetMemberConfigs(targetMember.Name, false, out var memberConfigs))
        {
            var memberConfig = memberConfigs.FirstOrDefault(x => x.Target.PathCount == 1);
            if (memberConfig != null && ResolveMemberConfigSourcePath(memberConfig, out var sourceMember))
            {
                return new MemberMappingInfo(sourceMember, new NonEmptyMemberPath(BuilderContext.Target, [targetMember]), memberConfig);
            }
        }

        if (ignoreCase && TryGetMemberConfigs(targetMember.Name, true, out memberConfigs))
        {
            var memberConfig = memberConfigs.FirstOrDefault(x => x.Target.PathCount == 1);
            if (memberConfig != null && ResolveMemberConfigSourcePath(memberConfig, out var sourceMember))
            {
                return new MemberMappingInfo(sourceMember, new NonEmptyMemberPath(BuilderContext.Target, [targetMember]), memberConfig);
            }
        }

        return null;
    }

    private bool TryGetConfiguredMemberMappingInfo(
        IMappableMember targetMember,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberMappingInfo? memberMappingInfo
    )
    {
        var valueMemberInfo = TryGetMemberValueMappingInfo(targetMember, ignoreCase);
        var configMemberInfo = TryGetMemberConfigMappingInfo(targetMember, ignoreCase);

        // If both exist (a value and a mapping config)
        // the one with the shorter target member path has the higher priority.
        // This prevents NRE's in the generated code.
        if (
            valueMemberInfo != null
            && configMemberInfo != null
            && valueMemberInfo.TargetMember.Path.Count > configMemberInfo.TargetMember.Path.Count
        )
        {
            memberMappingInfo = configMemberInfo;
            return true;
        }

        memberMappingInfo = valueMemberInfo ?? configMemberInfo;
        return memberMappingInfo != null;
    }

    private MemberMappingInfo? TryGetMemberValueMappingInfo(IMappableMember targetMember, bool ignoreCase)
    {
        if (TryGetMemberValueConfigs(targetMember.Name, false, out var memberValueConfigs))
        {
            var memberValueConfig = memberValueConfigs.FirstOrDefault(x => x.Target.PathCount == 1);
            if (memberValueConfig != null)
            {
                return new MemberMappingInfo(new NonEmptyMemberPath(BuilderContext.Target, [targetMember]), memberValueConfig);
            }
        }

        if (ignoreCase && TryGetMemberValueConfigs(targetMember.Name, true, out memberValueConfigs))
        {
            var memberValueConfig = memberValueConfigs.FirstOrDefault(x => x.Target.PathCount == 1);
            if (memberValueConfig != null)
            {
                return new MemberMappingInfo(new NonEmptyMemberPath(BuilderContext.Target, [targetMember]), memberValueConfig);
            }
        }

        return null;
    }
}
