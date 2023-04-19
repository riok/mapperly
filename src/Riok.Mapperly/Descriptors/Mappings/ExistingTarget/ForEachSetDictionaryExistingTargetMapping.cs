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
    private const string ExplicitCastVariableName = "targetDict";
    private const string KeyPropertyName = nameof(KeyValuePair<object, object>.Key);
    private const string ValuePropertyName = nameof(KeyValuePair<object, object>.Value);

    private readonly ITypeMapping _keyMapping;
    private readonly ITypeMapping _valueMapping;
    private readonly INamedTypeSymbol? _explicitCast;

    public ForEachSetDictionaryExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping keyMapping,
        ITypeMapping valueMapping,
        INamedTypeSymbol? explicitCast
    )
        : base(sourceType, targetType)
    {
        _keyMapping = keyMapping;
        _valueMapping = valueMapping;
        _explicitCast = explicitCast;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        if (_explicitCast != null)
        {
            var type = FullyQualifiedIdentifier(_explicitCast);
            var cast = CastExpression(type, target);

            var castedVariable = ctx.NameBuilder.New(ExplicitCastVariableName);
            target = IdentifierName(castedVariable);

            yield return LocalDeclarationStatement(DeclareVariable(castedVariable, cast));
        }

        var loopItemVariableName = ctx.NameBuilder.New(LoopItemVariableName);

        var convertedKeyExpression = _keyMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, KeyPropertyName)));
        var convertedValueExpression = _valueMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, ValuePropertyName)));

        var assignment = Assignment(ElementAccess(target, convertedKeyExpression), convertedValueExpression);

        yield return ForEachStatement(VarIdentifier, Identifier(loopItemVariableName), ctx.Source, Block(ExpressionStatement(assignment)));
    }
}
