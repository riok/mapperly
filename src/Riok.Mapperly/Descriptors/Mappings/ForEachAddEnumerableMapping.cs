using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Constructors;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.Capacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach enumerable mapping which works by creating a new target instance,
/// looping through the source, mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableMapping : NewInstanceObjectMemberMethodMapping, INewInstanceEnumerableMapping
{
    private readonly ForEachAddEnumerableExistingTargetMapping _existingTargetMapping;

    public ForEachAddEnumerableMapping(
        IInstanceConstructor? constructor,
        CollectionInfos collectionInfos,
        INewInstanceMapping elementMapping,
        bool enableReferenceHandling,
        string insertMethodName
    )
        : base(collectionInfos.Source.Type, collectionInfos.Target.Type, enableReferenceHandling)
    {
        _existingTargetMapping = new(collectionInfos, elementMapping, insertMethodName);
        if (constructor != null)
        {
            Constructor = constructor;
        }
    }

    public CollectionInfos CollectionInfos => _existingTargetMapping.CollectionInfos;

    public void AddCapacitySetter(ICapacitySetter capacitySetter) => _existingTargetMapping.AddCapacitySetter(capacitySetter);

    protected override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return base.BuildBody(ctx, target).Concat(_existingTargetMapping.Build(ctx, target));
    }
}
