using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable to array mapping which works by initialising an array, looping through the source,
/// mapping each element and adding it to the target array.
/// </summary>
public class ArrayForEachMapping : MethodMapping
{
    private const string TargetVariableName = "target";
    private const string LoopItemVariableName = "item";
    private const string LoopCounterName = "i";

    private readonly INewInstanceMapping _elementMapping;
    private readonly ITypeSymbol _targetArrayElementType;
    private readonly string _countPropertyName;

    public ArrayForEachMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        INewInstanceMapping elementMapping,
        ITypeSymbol targetArrayElementType,
        string countPropertyName
    )
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _targetArrayElementType = targetArrayElementType;
        _countPropertyName = countPropertyName;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);
        var loopCounterVariableName = ctx.NameBuilder.New(LoopCounterName);

        // var target = new T[source.Count];
        var sourceLengthArrayRank = ArrayRankSpecifier(
            SingletonSeparatedList<ExpressionSyntax>(MemberAccess(ctx.Source, _countPropertyName))
        );
        var targetInitializationValue = ArrayCreationExpression(
            ArrayType(FullyQualifiedIdentifier(_targetArrayElementType)).WithRankSpecifiers(SingletonList(sourceLengthArrayRank))
        );
        yield return DeclareLocalVariable(targetVariableName, targetInitializationValue);

        // var i = 0;
        yield return DeclareLocalVariable(loopCounterVariableName, IntLiteral(0));

        // target[i] = Map(item);
        var (loopItemCtx, loopItemVariableName) = ctx.WithNewSource(LoopItemVariableName);
        var convertedSourceItemExpression = _elementMapping.Build(loopItemCtx);

        var assignment = Assignment(
            ElementAccess(IdentifierName(targetVariableName), IdentifierName(loopCounterVariableName)),
            convertedSourceItemExpression
        );

        // i++;
        var counterIncrement = PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName(loopCounterVariableName));

        // target[i] = Map(item);
        // i++;
        var assignmentBlock = Block(ExpressionStatement(assignment), ExpressionStatement(counterIncrement));

        // foreach(var item in source)
        //{
        //   target[i] = Map(item);
        //   i++;
        //}
        yield return ForEachStatement(VarIdentifier, Identifier(loopItemVariableName), ctx.Source, assignmentBlock);

        // return target;
        yield return ReturnVariable(targetVariableName);
    }
}
