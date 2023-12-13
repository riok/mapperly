using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class ExistingTargetMappingBuilder(MappingCollection mappings)
{
    private delegate IExistingTargetMapping? BuildExistingTargetMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildExistingTargetMapping> _builders = new BuildExistingTargetMapping[]
    {
        NullableMappingBuilder.TryBuildExistingTargetMapping,
        DerivedTypeMappingBuilder.TryBuildExistingTargetMapping,
        DictionaryMappingBuilder.TryBuildExistingTargetMapping,
        SpanMappingBuilder.TryBuildExistingTargetMapping,
        MemoryMappingBuilder.TryBuildExistingTargetMapping,
        EnumerableMappingBuilder.TryBuildExistingTargetMapping,
        NewInstanceObjectMemberMappingBuilder.TryBuildExistingTargetMapping,
    };

    public IExistingTargetMapping? Find(TypeMappingKey mappingKey)
    {
        return mappings.FindExistingInstanceMapping(mappingKey);
    }

    public IExistingTargetMapping? Build(MappingBuilderContext ctx, bool resultIsReusable)
    {
        foreach (var mappingBuilder in _builders)
        {
            if (mappingBuilder(ctx) is not { } mapping)
                continue;

            if (resultIsReusable)
            {
                mappings.AddExistingTargetMapping(mapping, ctx.MappingKey.Configuration);
            }

            mappings.EnqueueToBuildBody(mapping, ctx);
            return mapping;
        }

        return null;
    }
}
