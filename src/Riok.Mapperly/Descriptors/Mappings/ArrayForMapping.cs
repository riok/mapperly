using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

public class ArrayForMapping : MethodMapping
{
    private const string TargetVariableName = "target";
    private const string LoopCounterName = "i";
    private const string ArrayLengthProperty = nameof(Array.Length);

    private readonly TypeMapping _elementMapping;
    private readonly ITypeSymbol _targetArrayElementType;

    public ArrayForMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        TypeMapping elementMapping,
        ITypeSymbol targetArrayElementType) : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _targetArrayElementType = targetArrayElementType;
    }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        // var target = new T[source.Length];
        var sourceLengthArrayRank = ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(MemberAccess(source, ArrayLengthProperty)));
        var targetInitializationValue = ArrayCreationExpression(
            ArrayType(IdentifierName(_targetArrayElementType.ToDisplayString()))
                .WithRankSpecifiers(SingletonList(sourceLengthArrayRank)));
        yield return DeclareLocalVariable(TargetVariableName, targetInitializationValue);

        // target[i] = Map(source[i]);
        var mappedIndexedSourceValue = _elementMapping.Build(ElementAccess(source, IdentifierName(LoopCounterName)));
        var assignment = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            ElementAccess(IdentifierName(TargetVariableName), IdentifierName(LoopCounterName)),
            mappedIndexedSourceValue);
        var assignmentBlock = Block(SingletonList<StatementSyntax>(ExpressionStatement(assignment)));

        // for(var i = 0; i < source.Length; i++)
        //   target[i] = Map(source[i]);
        yield return IncrementalForLoop(LoopCounterName, assignmentBlock, MemberAccess(source, ArrayLengthProperty));

        // return target;
        yield return ReturnVariable(TargetVariableName);
    }
}
