using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Used when target is some readonly field that implements IList
/// as normal assignment mapping will not work
/// </summary>
internal class IListReadonlyMapping : MethodMapping
{
    private const string ListCountName = "Count";
    private const string LoopCounterName = "i";
    private const string AddMethodName = "Add";

    private readonly ITypeMapping _elementMapping;
    private readonly ITypeSymbol _targetArrayElementType;

    public IListReadonlyMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        ITypeSymbol targetArrayElementType) : base(sourceType, targetType, RefKind.Ref)
    {
        _elementMapping = elementMapping;
        _targetArrayElementType = targetArrayElementType;
    }

    protected override ITypeSymbol? ReturnType => null;

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = DefaultReferenceHandlerParameterName;
        var loopCounterVariableName = ctx.NameBuilder.New(LoopCounterName);

        // target.Add(Map(source[i]));
        var forLoopBuilderCtx = ctx.WithSource(ElementAccess(ctx.Source, IdentifierName(loopCounterVariableName)));
        var mappedIndexedSourceValue = _elementMapping.Build(forLoopBuilderCtx);
        var add = Invocation(MemberAccess(targetVariableName, AddMethodName), mappedIndexedSourceValue);
        var assignmentBlock = Block(SingletonList<StatementSyntax>(ExpressionStatement(add)));

        // for(var i = 0; i < source.Length; i++)
        //      target.Add(Map(source[i]));
        yield return IncrementalForLoop(loopCounterVariableName, assignmentBlock, MemberAccess(ctx.Source, ListCountName));
    }
}
