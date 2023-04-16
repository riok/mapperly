using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a foreach dictionary mapping which works by looping through the source,
/// mapping each element and set it to the target collection.
/// </summary>
public class ForEachSetDictionaryExistingTargetMapping : ExistingTargetMapping
{
    private const string LoopItemVariableName = "item";
    private const string KeyPropertyName = nameof(KeyValuePair<object, object>.Key);
    private const string ValuePropertyName = nameof(KeyValuePair<object, object>.Value);

    private readonly ITypeMapping _keyMapping;
    private readonly ITypeMapping _valueMapping;

    public ForEachSetDictionaryExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping keyMapping,
        ITypeMapping valueMapping)
        : base(sourceType, targetType)
    {
        _keyMapping = keyMapping;
        _valueMapping = valueMapping;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var loopItemVariableName = ctx.NameBuilder.New(LoopItemVariableName);

        var convertedKeyExpression = _keyMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, KeyPropertyName)));
        var convertedValueExpression = _valueMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, ValuePropertyName)));

        var assignment = Assignment(
            ElementAccess(target, convertedKeyExpression),
            convertedValueExpression);

        return new StatementSyntax[]
        {
            ForEachStatement(
                VarIdentifier,
                Identifier(loopItemVariableName),
                ctx.Source,
                Block(ExpressionStatement(assignment))),
        };
    }
}
