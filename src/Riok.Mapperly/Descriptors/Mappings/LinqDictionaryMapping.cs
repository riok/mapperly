using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents an enumerable mapping which works by using linq (select + collect).
/// </summary>
public class LinqDicitonaryMapping : TypeMapping
{
    private const string LambdaParamName = "x";

    private const string KeyPropertyName = nameof(KeyValuePair<object, object>.Key);
    private const string ValuePropertyName = nameof(KeyValuePair<object, object>.Value);

    private readonly IMethodSymbol _collectMethod;
    private readonly ITypeMapping _keyMapping;
    private readonly ITypeMapping _valueMapping;

    public LinqDicitonaryMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        IMethodSymbol collectMethod,
        ITypeMapping keyMapping,
        ITypeMapping valueMapping
    )
        : base(sourceType, targetType)
    {
        _collectMethod = collectMethod;
        _keyMapping = keyMapping;
        _valueMapping = valueMapping;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var scopedNameBuilder = ctx.NameBuilder.NewScope();
        var lambdaParamName = scopedNameBuilder.New(LambdaParamName);

        // if key and value types do not change then use a simple call
        // ie: source.ToImmutableDictionary();
        if (_keyMapping.IsSynthetic && _valueMapping.IsSynthetic)
            return StaticInvocation(_collectMethod, ctx.Source);

        // create expressions mapping the key and value and then create the final expression
        // ie: source.ToImmutableDictionary(x => x.Key, x=> (int)x.Value);
        var keyMapExpression = _keyMapping.Build(ctx.WithSource(MemberAccess(lambdaParamName, KeyPropertyName)));
        var keyExpression = SimpleLambdaExpression(Parameter(Identifier(lambdaParamName))).WithExpressionBody(keyMapExpression);

        var valueMapExpression = _valueMapping.Build(ctx.WithSource(MemberAccess(lambdaParamName, ValuePropertyName)));
        var valueExpression = SimpleLambdaExpression(Parameter(Identifier(lambdaParamName))).WithExpressionBody(valueMapExpression);

        return StaticInvocation(_collectMethod, ctx.Source, keyExpression, valueExpression);
    }
}
