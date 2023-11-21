using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class MappingBuilder
{
    private delegate INewInstanceMapping? BuildMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildMapping> _builders = new BuildMapping[]
    {
        NullableMappingBuilder.TryBuildMapping,
        DerivedTypeMappingBuilder.TryBuildMapping,
        SpecialTypeMappingBuilder.TryBuildMapping,
        DirectAssignmentMappingBuilder.TryBuildMapping,
        QueryableMappingBuilder.TryBuildMapping,
        DictionaryMappingBuilder.TryBuildMapping,
        SpanMappingBuilder.TryBuildMapping,
        MemoryMappingBuilder.TryBuildMapping,
        EnumerableMappingBuilder.TryBuildMapping,
        ImplicitCastMappingBuilder.TryBuildMapping,
        ParseMappingBuilder.TryBuildMapping,
        CtorMappingBuilder.TryBuildMapping,
        StringToEnumMappingBuilder.TryBuildMapping,
        EnumToStringMappingBuilder.TryBuildMapping,
        EnumMappingBuilder.TryBuildMapping,
        DateTimeToDateOnlyMappingBuilder.TryBuildMapping,
        DateTimeToTimeOnlyMappingBuilder.TryBuildMapping,
        ExplicitCastMappingBuilder.TryBuildMapping,
        ToStringMappingBuilder.TryBuildMapping,
        NewInstanceObjectPropertyMappingBuilder.TryBuildMapping,
    };

    private readonly MappingCollection _mappings;

    public MappingBuilder(MappingCollection mappings)
    {
        _mappings = mappings;
    }

    /// <inheritdoc cref="MappingCollection.UserMappings"/>
    public IReadOnlyCollection<IUserMapping> UserMappings => _mappings.UserMappings;

    public INewInstanceMapping? Find(TypeMappingKey mapping) => _mappings.Find(mapping);

    public INewInstanceMapping? Build(MappingBuilderContext ctx, bool resultIsReusable)
    {
        foreach (var mappingBuilder in _builders)
        {
            if (mappingBuilder(ctx) is not { } mapping)
                continue;

            if (resultIsReusable)
            {
                _mappings.AddNewInstanceMapping(mapping, ctx.MappingKey.Configuration);
            }

            _mappings.EnqueueToBuildBody(mapping, ctx);
            return mapping;
        }

        return null;
    }
}
