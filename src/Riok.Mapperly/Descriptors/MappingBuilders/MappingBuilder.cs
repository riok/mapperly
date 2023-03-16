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
        QueryableMappingBuilder.TryBuildMapping,
        DictionaryMappingBuilder.TryBuildMapping,
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

    /// <inheritdoc cref="MappingBuilderContext.FindMapping"/>
    public ITypeMapping? Find(ITypeSymbol sourceType, ITypeSymbol targetType)
        => _mappings.Find(sourceType, targetType);

    public ITypeMapping? Build(MappingBuilderContext ctx, bool resultIsReusable)
    {
        foreach (var mappingBuilder in _builders)
        {
            if (mappingBuilder(ctx) is not { } mapping)
                continue;

            if (resultIsReusable)
            {
                _mappings.Add(mapping);
            }

            _mappings.EnqueueToBuildBody(mapping, ctx);
            return mapping;
        }

        return null;
    }
}
