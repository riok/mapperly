using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class ExistingTargetMappingBuilder
{
    private delegate IExistingTargetMapping? BuildExistingTargetMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildExistingTargetMapping> _builders = new BuildExistingTargetMapping[]
    {
        NullableMappingBuilder.TryBuildExistingTargetMapping,
        DictionaryMappingBuilder.TryBuildExistingTargetMapping,
        SpanMappingBuilder.TryBuildExistingTargetMapping,
        MemoryMappingBuilder.TryBuildExistingTargetMapping,
        EnumerableMappingBuilder.TryBuildExistingTargetMapping,
        NewInstanceObjectPropertyMappingBuilder.TryBuildExistingTargetMapping,
    };

    private readonly MappingCollection _mappings;

    public ExistingTargetMappingBuilder(MappingCollection mappings)
    {
        _mappings = mappings;
    }

    public IExistingTargetMapping? Find(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ImmutableEquatableArray<MethodParameter> parameters
    ) => _mappings.FindExistingInstanceMapping(sourceType, targetType, parameters);

    public IExistingTargetMapping? Build(MappingBuilderContext ctx, bool resultIsReusable)
    {
        foreach (var mappingBuilder in _builders)
        {
            if (mappingBuilder(ctx) is not { } mapping)
                continue;

            if (resultIsReusable)
            {
                _mappings.AddExistingTargetMapping(mapping);
            }

            _mappings.EnqueueToBuildBody(mapping, ctx);
            return mapping;
        }

        return null;
    }
}
