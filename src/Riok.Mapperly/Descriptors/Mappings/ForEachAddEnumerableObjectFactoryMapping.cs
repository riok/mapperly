using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach enumerable mapping which works by creating a new target instance via an object factory,
/// looping through the source, mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableObjectFactoryMapping(
    CollectionInfos collectionInfos,
    INewInstanceMapping elementMapping,
    ObjectFactory objectFactory,
    bool enableReferenceHandling,
    string insertMethodName
)
    : NewInstanceObjectFactoryMemberMapping(
        collectionInfos.Source.Type,
        collectionInfos.Target.Type,
        objectFactory,
        enableReferenceHandling
    ),
        IEnumerableMapping
{
    private readonly ForEachAddEnumerableExistingTargetMapping _existingTargetMapping =
        new(collectionInfos, elementMapping, insertMethodName);

    public CollectionInfos CollectionInfos => _existingTargetMapping.CollectionInfos;

    public void AddEnsureCapacity(EnsureCapacityInfo ensureCapacityInfo) => _existingTargetMapping.AddEnsureCapacity(ensureCapacityInfo);

    protected override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return base.BuildBody(ctx, target).Concat(_existingTargetMapping.Build(ctx, target));
    }
}
