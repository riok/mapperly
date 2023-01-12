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

    private readonly ITypeMapping _keyMapping;
    private readonly ITypeMapping _valueMapping;
    private readonly bool _sourceHasCount;
    private readonly ObjectFactory? _objectFactory;
    private readonly ITypeSymbol _typeToInstantiate;

    public ForEachAddDictionaryMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping keyMapping,
        ITypeMapping valueMapping,
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

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var loopItemVariableName = ctx.NameBuilder.New(LoopItemVariableName);

        var convertedKeyExpression = _keyMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, KeyValueKeyPropertyName)));
        var convertedValueExpression = _valueMapping.Build(ctx.WithSource(MemberAccess(loopItemVariableName, KeyValueValuePropertyName)));

        if (_objectFactory != null)
        {
            yield return DeclareLocalVariable(TargetVariableName, _objectFactory.CreateType(SourceType, _typeToInstantiate, ctx.Source));
        }
        else if (_sourceHasCount)
        {
            yield return CreateInstance(TargetVariableName, _typeToInstantiate, MemberAccess(ctx.Source, CountPropertyName));
        }
        else
        {
            yield return CreateInstance(TargetVariableName, _typeToInstantiate);
        }

        var addMethod = MemberAccess(TargetVariableName, AddMethodName);
        yield return ForEachStatement(
            VarIdentifier,
            Identifier(loopItemVariableName),
            ctx.Source,
            Block(ExpressionStatement(Invocation(addMethod, convertedKeyExpression, convertedValueExpression))));
        yield return ReturnVariable(TargetVariableName);
    }
}
