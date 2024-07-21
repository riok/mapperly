using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// The state of an ongoing member mapping matching process.
/// Contains discovered but unmapped members, ignored members, etc.
/// </summary>
/// <param name="unmappedSourceMemberNames">Source member names which are not used in a member mapping yet.</param>
/// <param name="unmappedAdditionalSourceMemberNames">Additional source member names (additional mapping method parameters) which are not used in a member mapping yet.</param>
/// <param name="unmappedTargetMemberNames">Target member names which are not used in a member mapping yet.</param>
/// <param name="targetMemberCaseMapping">A dictionary with all members of the target with a case-insensitive key comparer.</param>
/// <param name="targetMembers">All known target members.</param>
/// <param name="memberValueConfigsByRootTargetName">All value configurations by root target member names, which are not yet consumed.</param>
/// <param name="memberConfigsByRootTargetName">All configurations by root target member names, which are not yet consumed.</param>
/// <param name="ignoredSourceMemberNames">All ignored source members names.</param>
internal class MembersMappingState(
    HashSet<string> unmappedSourceMemberNames,
    HashSet<string> unmappedAdditionalSourceMemberNames,
    HashSet<string> unmappedTargetMemberNames,
    IReadOnlyDictionary<string, IMappableMember> additionalSourceMembers,
    IReadOnlyDictionary<string, string> targetMemberCaseMapping,
    Dictionary<string, IMappableMember> targetMembers,
    Dictionary<string, List<MemberValueMappingConfiguration>> memberValueConfigsByRootTargetName,
    Dictionary<string, List<MemberMappingConfiguration>> memberConfigsByRootTargetName,
    HashSet<string> ignoredSourceMemberNames
)
{
    private readonly Dictionary<string, IMappableMember> _aliasedSourceMembers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// All source member names that are not used in a member mapping (yet).
    /// </summary>
    private readonly HashSet<string> _unmappedSourceMemberNames = unmappedSourceMemberNames;

    /// <summary>
    /// All additional source member names (additional mapping method parameters) that are not used in a member mapping (yet).
    /// </summary>
    private readonly HashSet<string> _unmappedAdditionalSourceMemberNames = unmappedAdditionalSourceMemberNames;

    /// <summary>
    /// All target member names that are not used in a member mapping (yet).
    /// </summary>
    private readonly HashSet<string> _unmappedTargetMemberNames = unmappedTargetMemberNames;

    public IReadOnlyCollection<string> IgnoredSourceMemberNames => ignoredSourceMemberNames;

    /// <summary>
    /// Whether any member mapping has been added.
    /// </summary>
    public bool HasMemberMapping { get; private set; }

    /// <inheritdoc cref="_unmappedSourceMemberNames"/>
    public IEnumerable<string> UnmappedSourceMemberNames => _unmappedSourceMemberNames;

    /// <inheritdoc cref="_unmappedAdditionalSourceMemberNames"/>
    public IEnumerable<string> UnmappedAdditionalSourceMemberNames => _unmappedAdditionalSourceMemberNames;

    public IReadOnlyDictionary<string, IMappableMember> AdditionalSourceMembers => additionalSourceMembers;

    public IReadOnlyDictionary<string, IMappableMember> AliasedSourceMembers => _aliasedSourceMembers;

    public IEnumerable<MemberMappingConfiguration> UnusedMemberConfigs => memberConfigsByRootTargetName.Values.SelectMany(x => x);

    public IEnumerable<IMappableMember> EnumerateUnmappedTargetMembers() => _unmappedTargetMemberNames.Select(x => targetMembers[x]);

    public IEnumerable<IMappableMember> EnumerateUnmappedOrConfiguredTargetMembers()
    {
        return _unmappedTargetMemberNames
            .Concat(memberValueConfigsByRootTargetName.Keys)
            .Concat(memberConfigsByRootTargetName.Keys)
            .Distinct()
            .Select(targetMembers.GetValueOrDefault)
            .WhereNotNull();
    }

    public void TryAddSourceMemberAlias(string alias, IMappableMember member) => _aliasedSourceMembers.TryAdd(alias, member);

    public void MappingAdded() => HasMemberMapping = true;

    public void MappingAdded(MemberMappingInfo info, bool ignoreTargetCasing)
    {
        MappingAdded();
        SetMembersMapped(info, ignoreTargetCasing);
    }

    public void IgnoreMembers(IMappableMember member)
    {
        _unmappedSourceMemberNames.Remove(member.Name);
        _unmappedTargetMemberNames.Remove(member.Name);
        ignoredSourceMemberNames.Add(member.Name);

        if (!HasMemberConfig(member.Name))
        {
            targetMembers.Remove(member.Name);
        }
    }

    public void SetTargetMemberMapped(IMappableMember targetMember) => SetTargetMemberMapped(targetMember.Name);

    public void SetTargetMemberMapped(string targetName, bool ignoreCase = false)
    {
        _unmappedTargetMemberNames.Remove(targetName);

        if (ignoreCase && targetMemberCaseMapping.TryGetValue(targetName, out targetName))
        {
            _unmappedTargetMemberNames.Remove(targetName);
        }
    }

    public void SetMembersMapped(MemberMappingInfo info, bool ignoreTargetCasing)
    {
        SetTargetMemberMapped(info.TargetMember.Path[0].Name, ignoreTargetCasing);

        if (info.SourceMember != null)
        {
            SetSourceMemberMapped(info.SourceMember);
        }

        if (info.Configuration != null)
        {
            ConsumeMemberConfig(info.Configuration);
        }

        if (info.ValueConfiguration != null)
        {
            ConsumeMemberConfig(info.ValueConfiguration);
        }
    }

    public void ConsumeMemberConfig(MemberValueMappingConfiguration config) =>
        ConsumeMemberConfig(config, config.Target, memberValueConfigsByRootTargetName);

    public void ConsumeMemberConfig(MemberMappingConfiguration config) =>
        ConsumeMemberConfig(config, config.Target, memberConfigsByRootTargetName);

    public bool TryGetMemberConfigs(
        string targetMemberName,
        bool ignoreCase,
        [NotNullWhen(true)] out IReadOnlyList<MemberMappingConfiguration>? memberConfigs
    )
    {
        if (ignoreCase)
        {
            targetMemberName = targetMemberCaseMapping.GetValueOrDefault(targetMemberName, targetMemberName);
        }

        if (memberConfigsByRootTargetName.TryGetValue(targetMemberName, out var configs))
        {
            memberConfigs = configs;
            return true;
        }

        memberConfigs = null;
        return false;
    }

    public bool TryGetMemberValueConfigs(
        string targetMemberName,
        bool ignoreCase,
        [NotNullWhen(true)] out IReadOnlyList<MemberValueMappingConfiguration>? memberConfigs
    )
    {
        if (ignoreCase)
        {
            targetMemberName = targetMemberCaseMapping.GetValueOrDefault(targetMemberName, targetMemberName);
        }

        if (memberValueConfigsByRootTargetName.TryGetValue(targetMemberName, out var configs))
        {
            memberConfigs = configs;
            return true;
        }

        memberConfigs = null;
        return false;
    }

    private void SetSourceMemberMapped(SourceMemberPath sourcePath)
    {
        if (sourcePath.MemberPath.Path.FirstOrDefault() is not { } sourceMember)
        {
            // Assume all source members are used when the source object is used itself.
            _unmappedSourceMemberNames.Clear();
            return;
        }

        switch (sourcePath.Type)
        {
            case SourceMemberType.Member
            or SourceMemberType.MemberAlias:
                _unmappedSourceMemberNames.Remove(sourceMember.Name);
                break;
            case SourceMemberType.AdditionalMappingMethodParameter:
                _unmappedAdditionalSourceMemberNames.Remove(sourceMember.Name);
                break;
        }
    }

    private void ConsumeMemberConfig<T>(T config, StringMemberPath targetPath, Dictionary<string, List<T>> configsByRootName)
    {
        if (!configsByRootName.TryGetValue(targetPath.Path[0], out var configs))
            return;

        configs.Remove(config);
        if (configs.Count == 0)
        {
            configsByRootName.Remove(targetPath.Path[0]);
        }
    }

    private bool HasMemberConfig(string name) =>
        memberConfigsByRootTargetName.ContainsKey(name) || memberValueConfigsByRootTargetName.ContainsKey(name);
}
