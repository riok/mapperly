using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An abstract base implementation of <see cref="IMembersBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public abstract class MembersMappingBuilderContext<T>(MappingBuilderContext builderContext, T mapping) : IMembersBuilderContext<T>
    where T : IMapping
{
    private readonly MembersMappingState _state = MembersMappingStateBuilder.Build(builderContext);

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

    public void SetTargetMemberMapped(IMappableMember targetMember) => _state.SetTargetMemberMapped(targetMember);

    protected void SetTargetMemberMapped(string targetMemberName, bool ignoreCase = false) =>
        _state.SetTargetMemberMapped(targetMemberName, ignoreCase);

    public void SetMembersMapped(MemberMappingInfo memberInfo) => _state.SetMembersMapped(memberInfo, false);

    public void ConsumeMemberConfig(MemberMappingInfo members)
    {
        if (members.Configuration != null)
        {
            ConsumeMemberConfig(members.Configuration);
        }
    }

    protected void MappingAdded(MemberMappingInfo info, bool ignoreTargetCasing = false) => _state.MappingAdded(info, ignoreTargetCasing);

    protected void ConsumeMemberConfig(MemberMappingConfiguration config) => _state.ConsumeMemberConfig(config);

    public IEnumerable<MemberMappingInfo> MatchMembers(IMappableMember targetMember)
    {
        if (TryGetMemberConfigs(targetMember.Name, false, out var memberConfigs))
        {
            // return configs with shorter target member paths first
            // to prevent NRE's in the generated code
            return ResolveMemberMappingInfo(memberConfigs.ToList()).OrderBy(x => x.TargetMember.Path.Count);
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
        memberInfo = TryGetMemberConfigMappingInfo(targetMember, ignoreCase == true);
        if (memberInfo != null)
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

    protected virtual bool TryFindSourcePath(
        IReadOnlyList<IReadOnlyList<string>> pathCandidates,
        bool ignoreCase,
        [NotNullWhen(true)] out MemberPath? sourceMemberPath
    )
    {
        return BuilderContext.SymbolAccessor.TryFindMemberPath(
            Mapping.SourceType,
            pathCandidates,
            _state.IgnoredSourceMemberNames,
            ignoreCase,
            out sourceMemberPath
        );
    }

    protected bool IsIgnoredSourceMember(string sourceMemberName) => _state.IgnoredSourceMemberNames.Contains(sourceMemberName);

    private IReadOnlyList<MemberMappingInfo> ResolveMemberMappingInfo(IEnumerable<MemberMappingConfiguration> configs) =>
        configs.Select(ResolveMemberMappingInfo).WhereNotNull().ToList();

    private MemberMappingInfo? ResolveMemberMappingInfo(MemberMappingConfiguration config)
    {
        if (
            !BuilderContext.SymbolAccessor.TryFindMemberPath(Mapping.TargetType, config.Target.Path, out var foundMemberPath)
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

    private bool ResolveMemberConfigSourcePath(MemberMappingConfiguration config, [NotNullWhen(true)] out MemberPath? sourceMemberPath)
    {
        if (!BuilderContext.SymbolAccessor.TryFindMemberPath(Mapping.SourceType, config.Source.Path, out sourceMemberPath))
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingSourceMemberNotFound,
                config.Source.FullName,
                Mapping.SourceType
            );

            // consume this member config and prevent its further usage
            // as it is invalid, and a diagnostic has already been reported
            _state.ConsumeMemberConfig(config);
            return false;
        }

        return true;
    }

    private bool TryFindSourcePath(string targetMemberName, [NotNullWhen(true)] out MemberPath? sourceMemberPath, bool? ignoreCase = null)
    {
        ignoreCase ??= BuilderContext.Configuration.Mapper.PropertyNameMappingStrategy == PropertyNameMappingStrategy.CaseInsensitive;
        var pathCandidates = MemberPathCandidateBuilder.BuildMemberPathCandidates(targetMemberName).Select(cs => cs.ToList()).ToList();

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
            var memberConfig = memberConfigs.FirstOrDefault(x => x.Target.Path.Count == 1);
            if (memberConfig != null && ResolveMemberConfigSourcePath(memberConfig, out var sourceMember))
            {
                return new MemberMappingInfo(sourceMember, new NonEmptyMemberPath(BuilderContext.Target, [targetMember]), memberConfig);
            }
        }

        if (ignoreCase && TryGetMemberConfigs(targetMember.Name, true, out memberConfigs))
        {
            var memberConfig = memberConfigs.FirstOrDefault(x => x.Target.Path.Count == 1);
            if (memberConfig != null && ResolveMemberConfigSourcePath(memberConfig, out var sourceMember))
            {
                return new MemberMappingInfo(sourceMember, new NonEmptyMemberPath(BuilderContext.Target, [targetMember]), memberConfig);
            }
        }

        return null;
    }
}
