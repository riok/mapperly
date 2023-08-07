using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

public class ArrayForMapping : MethodMapping
{
    private const string TargetVariableName = "target";
    private const string LoopCounterName = "i";
    private const string ArrayLengthProperty = nameof(Array.Length);

    private readonly INewInstanceMapping _elementMapping;
    private readonly ITypeSymbol _targetArrayElementType;

    public ArrayForMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        INewInstanceMapping elementMapping,
        ITypeSymbol targetArrayElementType
    )
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _targetArrayElementType = targetArrayElementType;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);
        var loopCounterVariableName = ctx.NameBuilder.New(LoopCounterName);

        // var target = new T[source.Length];
        var sourceLengthArrayRank = ArrayRankSpecifier(
            SingletonSeparatedList<ExpressionSyntax>(MemberAccess(ctx.Source, ArrayLengthProperty))
        );
        var targetInitializationValue = ArrayCreationExpression(
            ArrayType(FullyQualifiedIdentifier(_targetArrayElementType)).WithRankSpecifiers(SingletonList(sourceLengthArrayRank))
        );
        yield return DeclareLocalVariable(targetVariableName, targetInitializationValue);

        // target[i] = Map(source[i]);
        var forLoopBuilderCtx = ctx.WithSource(ElementAccess(ctx.Source, IdentifierName(loopCounterVariableName)));
        var mappedIndexedSourceValue = _elementMapping.Build(forLoopBuilderCtx);
        var assignment = Assignment(
            ElementAccess(IdentifierName(targetVariableName), IdentifierName(loopCounterVariableName)),
            mappedIndexedSourceValue
        );
        var assignmentBlock = Block(SingletonList<StatementSyntax>(ExpressionStatement(assignment)));

        // for(var i = 0; i < source.Length; i++)
        //   target[i] = Map(source[i]);
        yield return IncrementalForLoop(loopCounterVariableName, assignmentBlock, MemberAccess(ctx.Source, ArrayLengthProperty));

        // return target;
        yield return ReturnVariable(targetVariableName);
    }
}
