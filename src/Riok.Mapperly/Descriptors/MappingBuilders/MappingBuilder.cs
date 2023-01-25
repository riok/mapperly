using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class MappingBuilder
{
    private delegate ITypeMapping? BuildMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildMapping> _builders = new BuildMapping[]
    {
        NullableMappingBuilder.TryBuildMapping,
        SpecialTypeMappingBuilder.TryBuildMapping,
        DirectAssignmentMappingBuilder.TryBuildMapping,
        DictionaryMappingBuilder.TryBuildMapping,
        EnumerableMappingBuilder.TryBuildMapping,
        ImplicitCastMappingBuilder.TryBuildMapping,
        ParseMappingBuilder.TryBuildMapping,
        CtorMappingBuilder.TryBuildMapping,
        StringToEnumMappingBuilder.TryBuildMapping,
        EnumToStringMappingBuilder.TryBuildMapping,
        EnumMappingBuilder.TryBuildMapping,
        ExplicitCastMappingBuilder.TryBuildMapping,
        ToStringMappingBuilder.TryBuildMapping,
        NewInstanceObjectPropertyMappingBuilder.TryBuildMapping,
    };

    private readonly DescriptorBuilder _descriptorBuilder;
    private readonly MappingCollection _mappings;

    public MappingBuilder(DescriptorBuilder descriptorBuilder, MappingCollection mappings)
    {
        _descriptorBuilder = descriptorBuilder;
        _mappings = mappings;
    }

    /// <inheritdoc cref="MappingBuilderContext.FindMapping"/>
    public ITypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => _mappings.FindMapping(sourceType, targetType);

    /// <inheritdoc cref="MappingBuilderContext.FindOrBuildMapping"/>
    public ITypeMapping? FindOrBuild(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (_mappings.FindMapping(sourceType, targetType) is { } foundMapping)
            return foundMapping;

        if (BuildDelegate(null, sourceType, targetType) is not { } mapping)
            return null;

        _mappings.AddMapping(mapping);
        return mapping;
    }

    /// <inheritdoc cref="MappingBuilderContext.BuildMappingWithUserSymbol"/>
    public ITypeMapping? BuildWithUserSymbol(
        ISymbol userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (BuildDelegate(userSymbol, sourceType, targetType) is not { } mapping)
            return null;

        _mappings.AddMapping(mapping);
        return mapping;
    }

    /// <inheritdoc cref="MappingBuilderContext.BuildDelegateMapping"/>
    public ITypeMapping? BuildDelegate(
        ISymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var ctx = new MappingBuilderContext(_descriptorBuilder, sourceType, targetType, userSymbol);
        foreach (var mappingBuilder in _builders)
        {
            if (mappingBuilder(ctx) is { } mapping)
            {
                _mappings.EnqueueMappingToBuildBody(mapping, ctx);
                return mapping;
            }
        }

        return null;
    }
}
