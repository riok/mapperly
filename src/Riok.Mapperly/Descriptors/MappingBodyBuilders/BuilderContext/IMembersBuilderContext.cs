using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

/// <summary>
/// Context to build member mappings.
/// </summary>
/// <typeparam name="T">The type of the mapping.</typeparam>
public interface IMembersBuilderContext<out T>
    where T : IMapping
{
    T Mapping { get; }

    MappingBuilderContext BuilderContext { get; }

    IReadOnlyCollection<string> IgnoredSourceMemberNames { get; }

    Dictionary<string, IMappableMember> TargetMembers { get; }

    Dictionary<string, List<PropertyMappingConfiguration>> MemberConfigsByRootTargetName { get; }

    void AddDiagnostics();

    NullMemberMapping BuildNullMemberMapping(MemberPath sourcePath, INewInstanceMapping delegateMapping, ITypeSymbol targetMemberType);
}
