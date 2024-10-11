using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.Capacity;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableExistingTargetMapping(
    CollectionInfos collectionInfos,
    INewInstanceMapping elementMapping,
    string insertMethodName
) : ObjectMemberExistingTargetMapping(collectionInfos.Source.Type, collectionInfos.Target.Type), IEnumerableMapping
{
    private const string LoopItemVariableName = "item";

    private ICapacitySetter? _capacitySetter;

    public CollectionInfos CollectionInfos => collectionInfos;

    public void AddCapacitySetter(ICapacitySetter capacitySetter) => _capacitySetter = capacitySetter;

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        foreach (var statement in base.Build(ctx, target))
        {
            yield return statement;
        }

        if (_capacitySetter != null)
        {
            yield return _capacitySetter.Build(ctx, target);
        }

        var (loopItemCtx, loopItemVariableName) = ctx.WithNewSource(LoopItemVariableName);
        var convertedSourceItemExpression = elementMapping.Build(loopItemCtx);
        var addMethod = MemberAccess(target, insertMethodName);
        var body = ctx.SyntaxFactory.Invocation(addMethod, convertedSourceItemExpression);
        yield return ctx.SyntaxFactory.ForEach(loopItemVariableName, ctx.Source, body);
    }
}
