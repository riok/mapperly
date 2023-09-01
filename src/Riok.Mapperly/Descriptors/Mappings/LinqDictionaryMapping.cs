using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping which works by using linq (select + collect).
/// </summary>
public class LinqDictionaryMapping : NewInstanceMapping
{
    private const string KeyPropertyName = nameof(KeyValuePair<object, object>.Key);
    private const string ValuePropertyName = nameof(KeyValuePair<object, object>.Value);

    private readonly string _collectMethod;
    private readonly INewInstanceMapping _keyMapping;
    private readonly INewInstanceMapping _valueMapping;

    public LinqDictionaryMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        string collectMethod,
        INewInstanceMapping keyMapping,
        INewInstanceMapping valueMapping
    )
        : base(sourceType, targetType)
    {
        _collectMethod = collectMethod;
        _keyMapping = keyMapping;
        _valueMapping = valueMapping;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // if key and value types do not change then use a simple call
        // ie: source.ToImmutableDictionary();
        if (_keyMapping.IsSynthetic && _valueMapping.IsSynthetic)
            return Invocation(_collectMethod, ctx.Source);

        // create expressions mapping the key and value and then create the final expression
        // ie: source.ToImmutableDictionary(x => x.Key, x => (int)x.Value);
        var (keyLambdaCtx, keyLambdaParamName) = ctx.WithNewScopedSource(src => MemberAccess(src, KeyPropertyName));
        var keyMapExpression = _keyMapping.Build(keyLambdaCtx);
        var keyExpression = Lambda(keyLambdaParamName, keyMapExpression);

        var (valueLambdaCtx, valueLambdaParamName) = ctx.WithNewScopedSource(src => MemberAccess(src, ValuePropertyName));
        var valueMapExpression = _valueMapping.Build(valueLambdaCtx);
        var valueExpression = Lambda(valueLambdaParamName, valueMapExpression);

        return Invocation(_collectMethod, ctx.Source, keyExpression, valueExpression);
    }
}
