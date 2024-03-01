using Microsoft.CodeAnalysis;
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
public abstract class MembersMappingBuilderContext<T> : IMembersBuilderContext<T>
    where T : IMapping
{
    private readonly HashSet<string> _unmappedSourceMemberNames;
    private readonly HashSet<string> _mappedAndIgnoredTargetMemberNames;
    private readonly HashSet<string> _mappedAndIgnoredSourceMemberNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedTargetMemberNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedSourceMemberNames;

    private bool _hasMemberMapping;

    protected MembersMappingBuilderContext(MappingBuilderContext builderContext, T mapping)
    {
        BuilderContext = builderContext;
        Mapping = mapping;
        MemberConfigsByRootTargetName = GetMemberConfigurations();

        _unmappedSourceMemberNames = GetSourceMemberNames();
        TargetMembers = GetTargetMembers();

        IgnoredSourceMemberNames = builderContext
            .Configuration.Members.IgnoredSources.Concat(GetIgnoredSourceMembers())
            .Concat(GetIgnoredObsoleteSourceMembers())
            .ToHashSet();
        var ignoredTargetMemberNames = builderContext
            .Configuration.Members.IgnoredTargets.Concat(GetIgnoredTargetMembers())
            .Concat(GetIgnoredObsoleteTargetMembers())
            .ToHashSet();

        _ignoredUnmatchedSourceMemberNames = InitIgnoredUnmatchedProperties(IgnoredSourceMemberNames, _unmappedSourceMemberNames);
        _ignoredUnmatchedTargetMemberNames = InitIgnoredUnmatchedProperties(
            builderContext.Configuration.Members.IgnoredTargets,
            TargetMembers.Keys
        );

        _unmappedSourceMemberNames.ExceptWith(IgnoredSourceMemberNames);

        MemberConfigsByRootTargetName = GetMemberConfigurations();

        // source and target properties may have been ignored and mapped explicitly
        _mappedAndIgnoredSourceMemberNames = MemberConfigsByRootTargetName
            .Values.SelectMany(v => v.Select(s => s.Source.Path.First()))
            .ToHashSet();
        _mappedAndIgnoredSourceMemberNames.IntersectWith(IgnoredSourceMemberNames);

        _mappedAndIgnoredTargetMemberNames = new HashSet<string>(ignoredTargetMemberNames);
        _mappedAndIgnoredTargetMemberNames.IntersectWith(MemberConfigsByRootTargetName.Keys);

        // remove explicitly mapped ignored targets from ignoredTargetMemberNames
        // then remove all ignored targets from TargetMembers, leaving unignored and explicitly mapped ignored members
        ignoredTargetMemberNames.ExceptWith(_mappedAndIgnoredTargetMemberNames);

        TargetMembers.RemoveRange(ignoredTargetMemberNames);
    }

    public MappingBuilderContext BuilderContext { get; }

    public T Mapping { get; }

    public IReadOnlyCollection<string> IgnoredSourceMemberNames { get; }

    public Dictionary<string, IMappableMember> TargetMembers { get; }

    public Dictionary<string, List<MemberMappingConfiguration>> MemberConfigsByRootTargetName { get; }

    public NullMemberMapping BuildNullMemberMapping(
        MemberPath sourcePath,
        INewInstanceMapping delegateMapping,
        ITypeSymbol targetMemberType
    )
    {
        var getterSourcePath = GetterMemberPath.Build(BuilderContext, sourcePath);

        var nullFallback = NullFallbackValue.Default;
        if (!delegateMapping.SourceType.IsNullable() && sourcePath.IsAnyNullable())
        {
            nullFallback = BuilderContext.GetNullFallbackValue(targetMemberType);
        }

        return new NullMemberMapping(delegateMapping, getterSourcePath, targetMemberType, nullFallback, !BuilderContext.IsExpression);
    }

    public void AddDiagnostics()
    {
        AddUnmatchedIgnoredTargetMembersDiagnostics();
        AddUnmatchedIgnoredSourceMembersDiagnostics();
        AddUnmatchedTargetMembersDiagnostics();
        AddUnmatchedSourceMembersDiagnostics();
        AddMappedAndIgnoredSourceMembersDiagnostics();
        AddMappedAndIgnoredTargetMembersDiagnostics();
        AddNoMemberMappedDiagnostic();
    }

    protected void SetSourceMemberMapped(MemberPath sourcePath)
    {
        _hasMemberMapping = true;
        _unmappedSourceMemberNames.Remove(sourcePath.Path.First().Name);
    }

    private HashSet<string> InitIgnoredUnmatchedProperties(IEnumerable<string> allProperties, IEnumerable<string> mappedProperties)
    {
        var unmatched = new HashSet<string>(allProperties);
        unmatched.ExceptWith(mappedProperties);
        return unmatched;
    }

    private IEnumerable<string> GetIgnoredObsoleteTargetMembers()
    {
        var obsoleteStrategy = BuilderContext.Configuration.Members.IgnoreObsoleteMembersStrategy;

        if (!obsoleteStrategy.HasFlag(IgnoreObsoleteMembersStrategy.Target))
            return Enumerable.Empty<string>();

        return BuilderContext
            .SymbolAccessor.GetAllAccessibleMappableMembers(Mapping.TargetType)
            .Where(x => BuilderContext.SymbolAccessor.HasAttribute<ObsoleteAttribute>(x.MemberSymbol))
            .Select(x => x.Name);
    }

    private IEnumerable<string> GetIgnoredObsoleteSourceMembers()
    {
        var obsoleteStrategy = BuilderContext.Configuration.Members.IgnoreObsoleteMembersStrategy;

        if (!obsoleteStrategy.HasFlag(IgnoreObsoleteMembersStrategy.Source))
            return Enumerable.Empty<string>();

        return BuilderContext
            .SymbolAccessor.GetAllAccessibleMappableMembers(Mapping.SourceType)
            .Where(x => BuilderContext.SymbolAccessor.HasAttribute<ObsoleteAttribute>(x.MemberSymbol))
            .Select(x => x.Name);
    }

    private IEnumerable<string> GetIgnoredTargetMembers()
    {
        return BuilderContext
            .SymbolAccessor.GetAllAccessibleMappableMembers(Mapping.TargetType)
            .Where(x => BuilderContext.SymbolAccessor.HasAttribute<MapperIgnoreMemberAttribute>(x.MemberSymbol))
            .Select(x => x.Name);
    }

    private IEnumerable<string> GetIgnoredSourceMembers()
    {
        return BuilderContext
            .SymbolAccessor.GetAllAccessibleMappableMembers(Mapping.SourceType)
            .Where(x => BuilderContext.SymbolAccessor.HasAttribute<MapperIgnoreMemberAttribute>(x.MemberSymbol))
            .Select(x => x.Name);
    }

    private HashSet<string> GetSourceMemberNames()
    {
        return BuilderContext.SymbolAccessor.GetAllAccessibleMappableMembers(Mapping.SourceType).Select(x => x.Name).ToHashSet();
    }

    private Dictionary<string, IMappableMember> GetTargetMembers()
    {
        return BuilderContext
            .SymbolAccessor.GetAllAccessibleMappableMembers(Mapping.TargetType)
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, List<MemberMappingConfiguration>> GetMemberConfigurations()
    {
        return BuilderContext
            .Configuration.Members.ExplicitMappings.GroupBy(x => x.Target.Path.First())
            .ToDictionary(x => x.Key, x => x.ToList());
    }

    private void AddUnmatchedIgnoredTargetMembersDiagnostics()
    {
        foreach (var notFoundIgnoredMember in _ignoredUnmatchedTargetMemberNames)
        {
            if (notFoundIgnoredMember.Contains(StringMemberPath.MemberAccessSeparator, StringComparison.Ordinal))
            {
                BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NestedIgnoredTargetMember, notFoundIgnoredMember, Mapping.TargetType);
                continue;
            }
            BuilderContext.ReportDiagnostic(DiagnosticDescriptors.IgnoredTargetMemberNotFound, notFoundIgnoredMember, Mapping.TargetType);
        }
    }

    private void AddUnmatchedIgnoredSourceMembersDiagnostics()
    {
        foreach (var notFoundIgnoredMember in _ignoredUnmatchedSourceMemberNames)
        {
            if (notFoundIgnoredMember.Contains(StringMemberPath.MemberAccessSeparator, StringComparison.Ordinal))
            {
                BuilderContext.ReportDiagnostic(DiagnosticDescriptors.NestedIgnoredSourceMember, notFoundIgnoredMember, Mapping.TargetType);
                continue;
            }
            BuilderContext.ReportDiagnostic(DiagnosticDescriptors.IgnoredSourceMemberNotFound, notFoundIgnoredMember, Mapping.SourceType);
        }
    }

    private void AddUnmatchedTargetMembersDiagnostics()
    {
        foreach (var memberConfig in MemberConfigsByRootTargetName.Values.SelectMany(x => x))
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound,
                memberConfig.Target.FullName,
                Mapping.TargetType
            );
        }
    }

    private void AddUnmatchedSourceMembersDiagnostics()
    {
        if (!BuilderContext.Configuration.Members.RequiredMappingStrategy.HasFlag(RequiredMappingStrategy.Source))
            return;

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

    private void AddMappedAndIgnoredTargetMembersDiagnostics()
    {
        foreach (var targetMemberName in _mappedAndIgnoredTargetMemberNames)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredTargetMemberExplicitlyMapped,
                targetMemberName,
                Mapping.TargetType
            );
        }
    }

    private void AddMappedAndIgnoredSourceMembersDiagnostics()
    {
        foreach (var sourceMemberName in _mappedAndIgnoredSourceMemberNames)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredSourceMemberExplicitlyMapped,
                sourceMemberName,
                Mapping.SourceType
            );
        }
    }

    private void AddNoMemberMappedDiagnostic()
    {
        if (!_hasMemberMapping)
        {
            BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.NoMemberMappings,
                BuilderContext.Source.ToDisplayString(),
                BuilderContext.Target.ToDisplayString()
            );
        }
    }
}
