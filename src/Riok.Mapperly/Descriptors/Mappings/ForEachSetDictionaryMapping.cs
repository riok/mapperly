using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach dictionary mapping which works by creating a new target instance,
/// looping through the source, mapping each element and setting it to the target collection.
/// </summary>
public class ForEachSetDictionaryMapping : NewInstanceObjectMemberMethodMapping, INewInstanceEnumerableMapping
{
    private readonly ForEachSetDictionaryExistingTargetMapping _existingTargetMapping;

    public ForEachSetDictionaryMapping(
        IInstanceConstructor? constructor,
        CollectionInfos collectionInfos,
        INewInstanceMapping keyMapping,
        INewInstanceMapping valueMapping,
        INamedTypeSymbol? explicitCast,
        bool enableReferenceHandling
    )
        : base(collectionInfos.Source.Type, collectionInfos.Target.Type, enableReferenceHandling)
    {
        _existingTargetMapping = new(collectionInfos, keyMapping, valueMapping, explicitCast);
        if (constructor != null)
        {
            Constructor = constructor;
        }
    }

    public CollectionInfos CollectionInfos => _existingTargetMapping.CollectionInfos;

    public void AddEnsureCapacity(EnsureCapacityInfo ensureCapacityInfo) => _existingTargetMapping.AddEnsureCapacity(ensureCapacityInfo);

    protected override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return base.BuildBody(ctx, target).Concat(_existingTargetMapping.Build(ctx, target));
    }
}
