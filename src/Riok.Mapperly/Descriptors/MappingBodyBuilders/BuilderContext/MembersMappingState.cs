using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// The state of an ongoing member mapping matching process.
/// Contains discovered but unmapped members, ignored members, etc.
/// </summary>
/// <param name="unmappedSourceMemberNames">Source member names which are not used in a member mapping yet.</param>
/// <param name="unmappedTargetMemberNames">Target member names which are not used in a member mapping yet.</param>
/// <param name="targetMemberCaseMapping">A dictionary with all members of the target with a case-insensitive key comparer.</param>
/// <param name="targetMembers">All known target members.</param>
/// <param name="memberConfigsByRootTargetName">All configurations by root target member names, which are not yet consumed.</param>
/// <param name="ignoredSourceMemberNames">All ignored source members names.</param>
internal class MembersMappingState(
    HashSet<string> unmappedSourceMemberNames,
    HashSet<string> unmappedTargetMemberNames,
    IReadOnlyDictionary<string, string> targetMemberCaseMapping,
    IReadOnlyDictionary<string, IMappableMember> targetMembers,
    Dictionary<string, List<MemberValueMappingConfiguration>> memberValueConfigsByRootTargetName,
    Dictionary<string, List<MemberMappingConfiguration>> memberConfigsByRootTargetName,
    IReadOnlyCollection<string> ignoredSourceMemberNames
)
{
    /// <summary>
    /// All source member names that are not used in a member mapping (yet).
    /// </summary>
    private readonly HashSet<string> _unmappedSourceMemberNames = unmappedSourceMemberNames;

    /// <summary>
    /// All target member names that are not used in a member mapping (yet).
    /// </summary>
    private readonly HashSet<string> _unmappedTargetMemberNames = unmappedTargetMemberNames;

    public IReadOnlyCollection<string> IgnoredSourceMemberNames { get; } = ignoredSourceMemberNames;

    /// <summary>
    /// Whether any member mapping has been added.
    /// </summary>
    public bool HasMemberMapping { get; private set; }

    public IEnumerable<string> UnmappedSourceMemberNames => _unmappedSourceMemberNames;

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

    public void MappingAdded(MemberMappingInfo info, bool ignoreTargetCasing)
    {
        HasMemberMapping = true;
        SetMembersMapped(info, ignoreTargetCasing);
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

    private void SetSourceMemberMapped(MemberPath sourcePath)
    {
        if (sourcePath.Path.FirstOrDefault() is { } sourceMember)
        {
            _unmappedSourceMemberNames.Remove(sourceMember.Name);
        }
        else
        {
            // Assume all source members are used when the source object is used itself.
            _unmappedSourceMemberNames.Clear();
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
}
