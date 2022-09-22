using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach dictionary mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddDictionaryMapping : MethodMapping
{
    private const string TargetVariableName = "target";
    private const string LoopItemVariableName = "item";
    private const string AddMethodName = "Add";
    private const string KeyValueKeyPropertyName = "Key";
    private const string KeyValueValuePropertyName = "Value";
    private const string CountPropertyName = "Count";

    private readonly TypeMapping _keyMapping;
    private readonly TypeMapping _valueMapping;
    private readonly bool _sourceHasCount;
    private readonly ObjectFactory? _objectFactory;
    private readonly ITypeSymbol _typeToInstantiate;

    public ForEachAddDictionaryMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        TypeMapping keyMapping,
        TypeMapping valueMapping,
        bool sourceHasCount,
        ITypeSymbol? typeToInstantiate = null,
        ObjectFactory? objectFactory = null)
        : base(sourceType, targetType)
    {
        _keyMapping = keyMapping;
        _valueMapping = valueMapping;
        _sourceHasCount = sourceHasCount;
        _objectFactory = objectFactory;
        _typeToInstantiate = typeToInstantiate ?? targetType;
    }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        var convertedKeyExpression = _keyMapping.Build(MemberAccess(LoopItemVariableName, KeyValueKeyPropertyName));
        var convertedValueExpression = _valueMapping.Build(MemberAccess(LoopItemVariableName, KeyValueValuePropertyName));

        if (_objectFactory != null)
        {
            yield return DeclareLocalVariable(TargetVariableName, _objectFactory.CreateType(SourceType, _typeToInstantiate, source));
        }
        else if (_sourceHasCount)
        {
            yield return CreateInstance(TargetVariableName, _typeToInstantiate, MemberAccess(source, CountPropertyName));
        }
        else
        {
            yield return CreateInstance(TargetVariableName, _typeToInstantiate);
        }

        var addMethod = MemberAccess(TargetVariableName, AddMethodName);
        yield return ForEachStatement(
            VarIdentifier,
            Identifier(LoopItemVariableName),
            source,
            Block(ExpressionStatement(Invocation(addMethod, convertedKeyExpression, convertedValueExpression))));
        yield return ReturnVariable(TargetVariableName);
    }
}
