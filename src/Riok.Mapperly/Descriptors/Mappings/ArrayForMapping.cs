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

    private readonly ITypeMapping _elementMapping;
    private readonly ITypeSymbol _targetArrayElementType;

    public ArrayForMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        ITypeSymbol targetArrayElementType) : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _targetArrayElementType = targetArrayElementType;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // var target = new T[source.Length];
        var sourceLengthArrayRank = ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(MemberAccess(ctx.Source, ArrayLengthProperty)));
        var targetInitializationValue = ArrayCreationExpression(
            ArrayType(IdentifierName(_targetArrayElementType.ToDisplayString()))
                .WithRankSpecifiers(SingletonList(sourceLengthArrayRank)));
        yield return DeclareLocalVariable(TargetVariableName, targetInitializationValue);

        // target[i] = Map(source[i]);
        var forLoopBuilderCtx = ctx.WithSource(ElementAccess(ctx.Source, IdentifierName(LoopCounterName)));
        var mappedIndexedSourceValue = _elementMapping.Build(forLoopBuilderCtx);
        var assignment = AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            ElementAccess(IdentifierName(TargetVariableName), IdentifierName(LoopCounterName)),
            mappedIndexedSourceValue);
        var assignmentBlock = Block(SingletonList<StatementSyntax>(ExpressionStatement(assignment)));

        // for(var i = 0; i < source.Length; i++)
        //   target[i] = Map(source[i]);
        yield return IncrementalForLoop(LoopCounterName, assignmentBlock, MemberAccess(ctx.Source, ArrayLengthProperty));

        // return target;
        yield return ReturnVariable(TargetVariableName);
    }
}
