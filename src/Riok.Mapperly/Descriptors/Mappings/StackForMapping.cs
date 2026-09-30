using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an indexable collection to stack mapping which controls the target order through the iteration direction.
/// </summary>
public class StackForMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    INewInstanceMapping elementMapping,
    IMemberGetter sourceCountAccessor,
    StackCloningStrategy stackCloningStrategy
) : NewInstanceMethodMapping(sourceType, targetType)
{
    private const string TargetVariableName = "target";
    private const string LoopCounterName = "i";
    private const string PushMethodName = nameof(Stack<>.Push);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);
        var loopCounterVariableName = ctx.NameBuilder.New(LoopCounterName);
        var sourceCount = sourceCountAccessor.BuildAccess(ctx.Source);

        // Pre-size the stack and push the indexable source without an intermediate buffer.
        // var target = new Stack<T>(source.Count);
        var targetInitializationValue = ctx.SyntaxFactory.CreateInstance(TargetType, sourceCount);
        yield return ctx.SyntaxFactory.DeclareLocalVariable(targetVariableName, targetInitializationValue);

        // Iterate backwards to preserve the source order or forwards to reverse it.
        // for (var i = ...)
        // {
        //     target.Push(Map(source[i]));
        // }
        var indexedSource = ElementAccess(ctx.Source, IdentifierName(loopCounterVariableName));
        var mappedSource = elementMapping.Build(ctx.WithSource(indexedSource).AddIndentation());
        var push = ctx.SyntaxFactory.Invocation(MemberAccess(targetVariableName, PushMethodName), mappedSource);
        yield return stackCloningStrategy == StackCloningStrategy.PreserveOrder
            ? ctx.SyntaxFactory.DecrementalForLoop(loopCounterVariableName, sourceCount, push)
            : ctx.SyntaxFactory.IncrementalForLoop(loopCounterVariableName, sourceCount, push);

        // return target;
        yield return ctx.SyntaxFactory.ReturnVariable(targetVariableName);
    }
}
