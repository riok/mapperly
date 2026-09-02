using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a count-known enumerable to stack mapping which controls the target order while mapping elements forwards.
/// </summary>
public class StackForEachMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    INewInstanceMapping elementMapping,
    ITypeSymbol targetElementType,
    IMemberGetter sourceCountAccessor,
    StackCloningStrategy stackCloningStrategy
) : NewInstanceMethodMapping(sourceType, targetType)
{
    private const string TargetVariableName = "target";
    private const string LoopItemVariableName = "item";
    private const string LoopCounterName = "i";
    private const string ArrayLengthProperty = nameof(Array.Length);
    private const string PushMethodName = nameof(Stack<>.Push);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        return stackCloningStrategy == StackCloningStrategy.PreserveOrder ? BuildPreserveOrderBody(ctx) : BuildReverseOrderBody(ctx);
    }

    private IEnumerable<StatementSyntax> BuildPreserveOrderBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);
        var loopCounterVariableName = ctx.NameBuilder.New(LoopCounterName);

        // A non-indexable source cannot be traversed backwards. Fill an array backwards so the Stack(IEnumerable<T>)
        // constructor reverses it again and preserves the source order.
        // var target = new T[source.Count];
        var targetInitializationValue = CreateArray(targetElementType, sourceCountAccessor.BuildAccess(ctx.Source));
        yield return ctx.SyntaxFactory.DeclareLocalVariable(targetVariableName, targetInitializationValue);

        // var i = target.Length;
        var targetLength = MemberAccess(IdentifierName(targetVariableName), ArrayLengthProperty);
        yield return ctx.SyntaxFactory.DeclareLocalVariable(loopCounterVariableName, targetLength);

        // foreach (var item in source)
        // {
        //     target[--i] = Map(item);
        // }
        var (loopItemCtx, loopItemVariableName) = ctx.WithNewSource(LoopItemVariableName);
        var convertedSourceItemExpression = elementMapping.Build(loopItemCtx.AddIndentation());
        var decrementedCounter = PrefixUnaryExpression(SyntaxKind.PreDecrementExpression, IdentifierName(loopCounterVariableName));
        var assignment = Assignment(ElementAccess(IdentifierName(targetVariableName), decrementedCounter), convertedSourceItemExpression);
        yield return ctx.SyntaxFactory.ForEach(loopItemVariableName, ctx.Source, assignment);

        // return new Stack<T>(target);
        var targetStack = ctx.SyntaxFactory.CreateInstance(TargetType, IdentifierName(targetVariableName));
        yield return ctx.SyntaxFactory.Return(targetStack);
    }

    private IEnumerable<StatementSyntax> BuildReverseOrderBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        // Reverse the source order by pushing mapped elements into a pre-sized stack in their enumeration order.
        // var target = new Stack<T>(source.Count);
        var sourceCount = sourceCountAccessor.BuildAccess(ctx.Source);
        var targetInitializationValue = ctx.SyntaxFactory.CreateInstance(TargetType, sourceCount);
        yield return ctx.SyntaxFactory.DeclareLocalVariable(targetVariableName, targetInitializationValue);

        // foreach (var item in source)
        // {
        //     target.Push(Map(item));
        // }
        var (loopItemCtx, loopItemVariableName) = ctx.WithNewSource(LoopItemVariableName);
        var convertedSourceItemExpression = elementMapping.Build(loopItemCtx.AddIndentation());
        var push = ctx.SyntaxFactory.Invocation(MemberAccess(targetVariableName, PushMethodName), convertedSourceItemExpression);
        yield return ctx.SyntaxFactory.ForEach(loopItemVariableName, ctx.Source, push);

        // return target;
        yield return ctx.SyntaxFactory.ReturnVariable(targetVariableName);
    }
}
