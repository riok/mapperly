using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
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
    private readonly HashSet<string> _mappedAndIgnoredTargetMemberNames;
    private readonly HashSet<string> _mappedAndIgnoredSourceMemberNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedTargetMemberNames;
    private readonly IReadOnlyCollection<string> _ignoredUnmatchedSourceMemberNames;

    protected MembersMappingBuilderContext(MappingBuilderContext builderContext, T mapping)
    {
        BuilderContext = builderContext;
        Mapping = mapping;
        MemberConfigsByRootTargetName = GetMemberConfigurations();

        _unmappedSourceMemberNames = GetSourceMemberNames();
        TargetMembers = GetTargetMembers();

        IgnoredSourceMemberNames = builderContext.Configuration.Properties.IgnoredSources
            .Concat(GetIgnoredObsoleteSourceMembers())
            .ToHashSet();
        var ignoredTargetMemberNames = builderContext.Configuration.Properties.IgnoredTargets
            .Concat(GetIgnoredObsoleteTargetMembers())
            .Concat(GetComplexTypes())
            .ToHashSet();

        _ignoredUnmatchedSourceMemberNames = InitIgnoredUnmatchedProperties(IgnoredSourceMemberNames, _unmappedSourceMemberNames);
        _ignoredUnmatchedTargetMemberNames = InitIgnoredUnmatchedProperties(
            builderContext.Configuration.Properties.IgnoredTargets,
            TargetMembers.Keys
        );

        _unmappedSourceMemberNames.ExceptWith(IgnoredSourceMemberNames);

        MemberConfigsByRootTargetName = GetMemberConfigurations();

        // source and target properties may have been ignored and mapped explicitly
        _mappedAndIgnoredSourceMemberNames = MemberConfigsByRootTargetName.Values
            .SelectMany(v => v.Select(s => s.Source.Path.First()))
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

    public Dictionary<string, List<PropertyMappingConfiguration>> MemberConfigsByRootTargetName { get; }

    public void AddDiagnostics()
    {
        AddUnmatchedIgnoredTargetMembersDiagnostics();
        AddUnmatchedIgnoredSourceMembersDiagnostics();
        AddUnmatchedTargetMembersDiagnostics();
        AddUnmatchedSourceMembersDiagnostics();
        AddMappedAndIgnoredSourceMembersDiagnostics();
        AddMappedAndIgnoredTargetMembersDiagnostics();
    }

    protected void SetSourceMemberMapped(MemberPath sourcePath) => _unmappedSourceMemberNames.Remove(sourcePath.Path.First().Name);

    private HashSet<string> InitIgnoredUnmatchedProperties(IEnumerable<string> allProperties, IEnumerable<string> mappedProperties)
    {
        var unmatched = new HashSet<string>(allProperties);
        unmatched.ExceptWith(mappedProperties);
        return unmatched;
    }

    private IEnumerable<string> GetIgnoredObsoleteTargetMembers()
    {
        var obsoleteStrategy = BuilderContext.Configuration.Properties.IgnoreObsoleteMembersStrategy;

        if (!obsoleteStrategy.HasFlag(IgnoreObsoleteMembersStrategy.Target))
            return Enumerable.Empty<string>();

        return BuilderContext.SymbolAccessor
            .GetAllAccessibleMappableMembers(Mapping.TargetType)
            .Where(x => BuilderContext.SymbolAccessor.HasAttribute<ObsoleteAttribute>(x.MemberSymbol))
            .Select(x => x.Name);
    }

    private IEnumerable<string> GetComplexTypes()
    {
        var mapOnlyPrimitives = BuilderContext.Configuration.Properties.MapOnlyPrimitives;

        if (!mapOnlyPrimitives)
            return Enumerable.Empty<string>();

        return BuilderContext.SymbolAccessor
            .GetAllAccessibleMappableMembers(Mapping.TargetType)
            .Where(x => x.Type.IsComplexType(BuilderContext.Types))
            .Select(x => x.Name);
    }

    private IEnumerable<string> GetIgnoredObsoleteSourceMembers()
    {
        var obsoleteStrategy = BuilderContext.Configuration.Properties.IgnoreObsoleteMembersStrategy;

        if (!obsoleteStrategy.HasFlag(IgnoreObsoleteMembersStrategy.Source))
            return Enumerable.Empty<string>();

        return BuilderContext.SymbolAccessor
            .GetAllAccessibleMappableMembers(Mapping.SourceType)
            .Where(x => BuilderContext.SymbolAccessor.HasAttribute<ObsoleteAttribute>(x.MemberSymbol))
            .Select(x => x.Name);
    }

    private HashSet<string> GetSourceMemberNames()
    {
        return BuilderContext.SymbolAccessor.GetAllAccessibleMappableMembers(Mapping.SourceType).Select(x => x.Name).ToHashSet();
    }

    private Dictionary<string, IMappableMember> GetTargetMembers()
    {
        return BuilderContext.SymbolAccessor
            .GetAllAccessibleMappableMembers(Mapping.TargetType)
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, List<PropertyMappingConfiguration>> GetMemberConfigurations()
    {
        return BuilderContext.Configuration.Properties.ExplicitMappings
            .GroupBy(x => x.Target.Path.First())
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
                memberConfig.Target.FullName,
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
}

public static class TypeExtensions
{
    public static bool IsComplexType(this ITypeSymbol type, WellKnownTypes knowTypes)
    {
        return type.TypeKind switch
        {
            // check if it's an IEnumerable of primitives
            TypeKind.Class
                => type.Name != "string"
                    && (
                        (
                            type is INamedTypeSymbol namedType
                            && namedType.AllInterfaces.Any(s => string.Equals(s.Name, "IEnumerable", StringComparison.OrdinalIgnoreCase))
                            && namedType.TypeArguments.Count() > 0
                            && namedType.TypeArguments[0].IsComplexType(knowTypes)
                        )
                        || !(
                            type is INamedTypeSymbol namedType2
                            && namedType2.AllInterfaces.Any(s => string.Equals(s.Name, "IEnumerable", StringComparison.OrdinalIgnoreCase))
                        )
                    ),
            TypeKind.Struct => false,
            TypeKind.Enum => false,
            TypeKind.Delegate => true,
            TypeKind.Interface => true,
            // check if it's an array of primitives
            TypeKind.Array
                => type is IArrayTypeSymbol arrayType && arrayType.ElementType.IsComplexType(knowTypes)
                    || type is not IArrayTypeSymbol arrayType2,
            TypeKind.Dynamic => true,
            TypeKind.Error => true,
            TypeKind.Pointer => true,
            TypeKind.Submission => false,
            TypeKind.Module => true,
            TypeKind.TypeParameter => false,
            TypeKind.Unknown => false,
            _ => false
        };
    }
}
