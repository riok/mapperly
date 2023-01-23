using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class ExistingTargetMappingBuilder
{
    private delegate IExistingTargetMapping? BuildExistingTargetMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildExistingTargetMapping> _builders = new BuildExistingTargetMapping[]
    {
        NullableMappingBuilder.TryBuildExistingTargetMapping,
        DictionaryMappingBuilder.TryBuildExistingTargetMapping,
        EnumerableMappingBuilder.TryBuildExistingTargetMapping,
        NewInstanceObjectPropertyMappingBuilder.TryBuildExistingTargetMapping,
    };

    private readonly MappingCollection _mappings;
    private readonly DescriptorBuilder _descriptorBuilder;

    public ExistingTargetMappingBuilder(DescriptorBuilder descriptorBuilder, MappingCollection mappings)
    {
        _descriptorBuilder = descriptorBuilder;
        _mappings = mappings;
    }

    /// <inheritdoc cref="MappingBuilderContext.FindOrBuildExistingTargetMapping"/>
    public IExistingTargetMapping? FindOrBuild(
        ISymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        return _mappings.FindExistingInstanceMapping(sourceType, targetType)
            ?? Build(userSymbol, sourceType, targetType);
    }

    private IExistingTargetMapping? Build(
        ISymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var ctx = new MappingBuilderContext(_descriptorBuilder, sourceType, targetType, userSymbol);
        foreach (var mappingBuilder in _builders)
        {
            if (mappingBuilder(ctx) is { } mapping)
            {
                _mappings.AddExistingTargetMapping(mapping);
                _mappings.EnqueueMappingToBuildBody(mapping, ctx);
                return mapping;
            }
        }

        return null;
    }
}
