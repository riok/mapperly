using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.Enumerables.Capacity;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a foreach dictionary mapping which works by looping through the source,
/// mapping each element and set it to the target collection.
/// </summary>
public class ForEachSetDictionaryExistingTargetMapping(
    CollectionInfos collectionInfos,
    INewInstanceMapping keyMapping,
    INewInstanceMapping valueMapping,
    INamedTypeSymbol? explicitCast
) : ObjectMemberExistingTargetMapping(collectionInfos.Source.Type, collectionInfos.Target.Type), IEnumerableMapping
{
    private const string LoopItemVariableName = "item";
    private const string ExplicitCastVariableName = "targetDict";
    private const string KeyPropertyName = nameof(KeyValuePair<object, object>.Key);
    private const string ValuePropertyName = nameof(KeyValuePair<object, object>.Value);

    private ICapacitySetter? _capacitySetter;

    public CollectionInfos CollectionInfos => collectionInfos;

    public void AddCapacitySetter(ICapacitySetter capacitySetter) => _capacitySetter = capacitySetter;

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        foreach (var statement in base.Build(ctx, target))
        {
            yield return statement;
        }

        if (explicitCast != null)
        {
            var type = FullyQualifiedIdentifier(explicitCast);
            var cast = CastExpression(type, target);

            var castedVariable = ctx.NameBuilder.New(ExplicitCastVariableName);
            target = IdentifierName(castedVariable);

            yield return ctx.SyntaxFactory.DeclareLocalVariable(castedVariable, cast);
        }

        if (_capacitySetter != null)
        {
            yield return _capacitySetter.Build(ctx, target);
        }

        var loopItemVariableName = ctx.NameBuilder.New(LoopItemVariableName);

        var convertedKeyExpression = keyMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, KeyPropertyName)));
        var convertedValueExpression = valueMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, ValuePropertyName)));

        var assignment = Assignment(ElementAccess(target, convertedKeyExpression), convertedValueExpression);

        yield return ctx.SyntaxFactory.ForEach(loopItemVariableName, ctx.Source, assignment);
    }
}
