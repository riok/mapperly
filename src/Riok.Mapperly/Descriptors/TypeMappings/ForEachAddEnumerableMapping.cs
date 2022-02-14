using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableMapping : MethodMapping
{
    private const string TargetVariableName = "target";
    private const string LoopItemVariableName = "item";
    private const string AddMethodName = "Add";

    private readonly TypeMapping _elementMapping;

    public ForEachAddEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        TypeMapping elementMapping)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
    }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        var convertedSourceItemExpression = _elementMapping.Build(IdentifierName(LoopItemVariableName));
        yield return CreateInstance(TargetVariableName, TargetType);
        var addMethod = MemberAccess(TargetVariableName, AddMethodName);
        yield return ForEachStatement(
            VarIdentifier,
            Identifier(LoopItemVariableName),
            source,
            Block(ExpressionStatement(Invocation(addMethod, convertedSourceItemExpression))));
        yield return ReturnVariable(TargetVariableName);
    }
}
