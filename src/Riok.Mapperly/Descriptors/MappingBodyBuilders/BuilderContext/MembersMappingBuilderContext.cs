using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// An abstract base implementation of <see cref="IMembersBuilderContext{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public abstract class MembersMappingBuilderContext<T> : IMembersBuilderContext<T>
    where T : IMapping
{
    private readonly HashSet<string> _unmappedSourceMemberNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedTargetMemberNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedSourceMemberNames;

    protected MembersMappingBuilderContext(MappingBuilderContext builderContext, T mapping)
    {
        BuilderContext = builderContext;
        Mapping = mapping;

        _unmappedSourceMemberNames = GetSourceMemberNames();
        TargetMembers = GetTargetMembers();

        var ignoredTargetMemberNames = GetIgnoredTargetMembers();
        IgnoredSourceMemberNames = GetIgnoredSourceMembers();

        _ignoredUnmatchedSourceMemberNames = InitIgnoredUnmatchedProperties(IgnoredSourceMemberNames, _unmappedSourceMemberNames);
        _ignoredUnmatchedTargetMemberNames = InitIgnoredUnmatchedProperties(ignoredTargetMemberNames, TargetMembers.Keys);

        _unmappedSourceMemberNames.ExceptWith(IgnoredSourceMemberNames);
        TargetMembers.RemoveRange(ignoredTargetMemberNames);

        MemberConfigsByRootTargetName = GetMemberConfigurations();
    }

    public MappingBuilderContext BuilderContext { get; }

    public T Mapping { get; }

    public IReadOnlyCollection<string> IgnoredSourceMemberNames { get; }

    public Dictionary<string, IMappableMember> TargetMembers { get; }

    public Dictionary<string, List<MapPropertyAttribute>> MemberConfigsByRootTargetName { get; }

    public void AddDiagnostics()
    {
        AddUnmatchedIgnoredTargetMembersDiagnostics();
        AddUnmatchedIgnoredSourceMembersDiagnostics();
        AddUnmatchedTargetMembersDiagnostics();
        AddUnmatchedSourceMembersDiagnostics();
    }

    protected void SetSourceMemberMapped(MemberPath sourcePath) => _unmappedSourceMemberNames.Remove(sourcePath.Path.First().Name);

    private HashSet<string> InitIgnoredUnmatchedProperties(IEnumerable<string> allProperties, IEnumerable<string> mappedProperties)
    {
        var unmatched = new HashSet<string>(allProperties);
        unmatched.ExceptWith(mappedProperties);
        return unmatched;
    }

    private HashSet<string> GetIgnoredTargetMembers()
    {
        return BuilderContext
            .ListConfiguration<MapperIgnoreTargetAttribute>()
            .Select(x => x.Target)
            .Concat(BuilderContext.ListConfiguration<MapperIgnoreTargetsAttribute>().SelectMany(x => x.Targets))
            // deprecated MapperIgnoreAttribute, but it is still supported by Mapperly.
#pragma warning disable CS0618
            .Concat(BuilderContext.ListConfiguration<MapperIgnoreAttribute>().Select(x => x.Target))
#pragma warning restore CS0618
            .ToHashSet();
    }

    private HashSet<string> GetIgnoredSourceMembers()
    {
        return BuilderContext
            .ListConfiguration<MapperIgnoreSourceAttribute>()
            .Select(x => x.Source)
            .Concat(BuilderContext.ListConfiguration<MapperIgnoreSourcesAttribute>().SelectMany(x => x.Sources))
            .ToHashSet();
    }

    private HashSet<string> GetSourceMemberNames()
    {
        return Mapping.SourceType.GetAccessibleMappableMembers().Select(x => x.Name).ToHashSet();
    }

    private Dictionary<string, IMappableMember> GetTargetMembers()
    {
        return Mapping.TargetType.GetAccessibleMappableMembers().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, List<MapPropertyAttribute>> GetMemberConfigurations()
    {
        return BuilderContext
            .ListConfiguration<MapPropertyAttribute>()
            .GroupBy(x => x.Target.First())
            .ToDictionary(x => x.Key, x => x.ToList());
    }

    private void AddUnmatchedIgnoredTargetMembersDiagnostics()
    {
        foreach (var notFoundIgnoredMember in _ignoredUnmatchedTargetMemberNames)
        {
            BuilderContext.ReportDiagnostic(DiagnosticDescriptors.IgnoredTargetMemberNotFound, notFoundIgnoredMember, Mapping.TargetType);
        }
    }

    private void AddUnmatchedIgnoredSourceMembersDiagnostics()
    {
        foreach (var notFoundIgnoredMember in _ignoredUnmatchedSourceMemberNames)
        {
            BuilderContext.ReportDiagnostic(DiagnosticDescriptors.IgnoredSourceMemberNotFound, notFoundIgnoredMember, Mapping.SourceType);
        }
    }

    private void AddUnmatchedTargetMembersDiagnostics()
    {
        foreach (var memberConfig in MemberConfigsByRootTargetName.Values.SelectMany(x => x))
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound,
                memberConfig.TargetFullName,
                Mapping.TargetType
            );
        }
    }

    private void AddUnmatchedSourceMembersDiagnostics()
    {
        foreach (var sourceMemberName in _unmappedSourceMemberNames)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                sourceMemberName,
                Mapping.SourceType,
                Mapping.TargetType
            );
        }
    }
}
