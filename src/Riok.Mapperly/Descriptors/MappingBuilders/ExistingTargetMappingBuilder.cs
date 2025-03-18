using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class ExistingTargetMappingBuilder(MappingCollection mappings, MapperDeclaration mapperDeclaration)
{
    private readonly HashSet<string> _resolvedMappingNames = [];

    private delegate IExistingTargetMapping? BuildExistingTargetMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildExistingTargetMapping> _builders =
    [
        UseNamedMappingBuilder.TryBuildExistingTargetMapping,
        NullableMappingBuilder.TryBuildExistingTargetMapping,
        DerivedTypeMappingBuilder.TryBuildExistingTargetMapping,
        DictionaryMappingBuilder.TryBuildExistingTargetMapping,
        SpanMappingBuilder.TryBuildExistingTargetMapping,
        MemoryMappingBuilder.TryBuildExistingTargetMapping,
        EnumerableMappingBuilder.TryBuildExistingTargetMapping,
        NewInstanceObjectMemberMappingBuilder.TryBuildExistingTargetMapping,
    ];

    public IExistingTargetMapping? Find(TypeMappingKey mappingKey)
    {
        return mappings.FindExistingInstanceMapping(mappingKey);
    }

    public IExistingTargetMapping? FindOrResolveNamed(SimpleMappingBuilderContext ctx, string name, out bool ambiguousName)
    {
        if (!ctx.Configuration.Mapper.AutoUserMappings && _resolvedMappingNames.Add(name))
        {
            // all user-defined mappings are already discovered
            // resolve user-implemented mappings which were not discovered in the initialization discovery
            // since no UserMappingAttribute was present
            var namedMappings = UserMethodMappingExtractor.ExtractNamedUserImplementedExistingInstanceMappings(
                ctx,
                mapperDeclaration.Symbol,
                name
            );
            mappings.AddNamedExistingInstanceUserMappings(name, namedMappings);
        }

        return mappings.FindExistingInstanceNamedMapping(name, out ambiguousName);
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
