using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach dictionary mapping which works by creating a new target instance via an object factory,
/// looping through the source, mapping each element and adding it to the target collection.
/// </summary>
public class ForEachSetDictionaryObjectFactoryMapping(
    CollectionInfos collectionInfos,
    INewInstanceMapping keyMapping,
    INewInstanceMapping valueMapping,
    INamedTypeSymbol? explicitCast,
    ObjectFactory objectFactory,
    bool enableReferenceHandling
)
    : NewInstanceObjectFactoryMemberMapping(
        collectionInfos.Source.Type,
        collectionInfos.Target.Type,
        objectFactory,
        enableReferenceHandling
    ),
        IEnumerableMapping
{
    private readonly ForEachSetDictionaryExistingTargetMapping _existingTargetMapping =
        new(collectionInfos, keyMapping, valueMapping, explicitCast);

    public CollectionInfos CollectionInfos => _existingTargetMapping.CollectionInfos;

    public void AddEnsureCapacity(EnsureCapacityInfo ensureCapacityInfo) => _existingTargetMapping.AddEnsureCapacity(ensureCapacityInfo);

    protected override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return base.BuildBody(ctx, target).Concat(_existingTargetMapping.Build(ctx, target));
    }
}
