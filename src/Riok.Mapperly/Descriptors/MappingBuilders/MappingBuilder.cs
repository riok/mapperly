using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class MappingBuilder(MappingCollection mappings, MapperDeclaration mapperDeclaration)
{
    private HashSet<string> _resolvedMappingNames = new();

    private delegate INewInstanceMapping? BuildMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildMapping> _builders = new BuildMapping[]
    {
        UseNamedMappingBuilder.TryBuildMapping,
        NullableMappingBuilder.TryBuildMapping,
        DerivedTypeMappingBuilder.TryBuildMapping,
        ToObjectMappingBuilder.TryBuildMapping,
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
        NewInstanceObjectMemberMappingBuilder.TryBuildMapping,
    };

    /// <inheritdoc cref="MappingCollection.UserMappings"/>
    public IReadOnlyCollection<IUserMapping> UserMappings => mappings.UserMappings;

    public INewInstanceMapping? Find(TypeMappingKey mapping) => mappings.FindNewInstanceMapping(mapping);

    public INewInstanceMapping? FindOrResolveNamed(SimpleMappingBuilderContext ctx, string name, out bool ambiguousName)
    {
        if (!ctx.Configuration.Mapper.AutoUserMappings && _resolvedMappingNames.Add(name))
        {
            // all user-defined mappings are already discovered
            // resolve user-implemented mappings which were not discovered in the initialization discovery
            // since no UserMappingAttribute was present
            var namedMappings = UserMethodMappingExtractor.ExtractNamedUserImplementedNewInstanceMappings(
                ctx,
                mapperDeclaration.Symbol,
                name
            );
            mappings.AddNamedNewInstanceUserMappings(name, namedMappings);
        }

        return mappings.FindNamedNewInstanceMapping(name, out ambiguousName);
    }

    public INewInstanceMapping? Build(MappingBuilderContext ctx, bool resultIsReusable)
    {
        foreach (var mappingBuilder in _builders)
        {
            if (mappingBuilder(ctx) is not { } mapping)
                continue;

            if (resultIsReusable)
            {
                mappings.AddNewInstanceMapping(mapping, ctx.MappingKey.Configuration);
            }

            mappings.EnqueueToBuildBody(mapping, ctx);
            return mapping;
        }

        return null;
    }
}
