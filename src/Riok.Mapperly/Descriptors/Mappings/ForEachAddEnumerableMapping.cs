using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableMapping : MethodMapping
{
    private const string TargetVariableName = "target";
    private const string LoopItemVariableName = "item";
    private const string AddMethodName = "Add";

    private readonly ITypeMapping _elementMapping;
    private readonly ObjectFactory? _objectFactory;

    public ForEachAddEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        ObjectFactory? objectFactory)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _objectFactory = objectFactory;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        yield return _objectFactory == null
            ? CreateInstance(TargetVariableName, TargetType)
            : DeclareLocalVariable(TargetVariableName, _objectFactory.CreateType(SourceType, TargetType, ctx.Source));

        var convertedSourceItemExpression = _elementMapping.Build(ctx.WithSource(LoopItemVariableName));
        var addMethod = MemberAccess(TargetVariableName, AddMethodName);
        yield return ForEachStatement(
            VarIdentifier,
            Identifier(LoopItemVariableName),
            ctx.Source,
            Block(ExpressionStatement(Invocation(addMethod, convertedSourceItemExpression))));
        yield return ReturnVariable(TargetVariableName);
    }
}
