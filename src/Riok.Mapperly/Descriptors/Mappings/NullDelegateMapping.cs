using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Null aware delegate mapping. Abstracts handling null values of the delegated mapping.
/// </summary>
public class NullDelegateMapping : TypeMapping
{
    private const string NullableValueProperty = "Value";

    private readonly TypeMapping _delegateMapping;
    private readonly NullFallbackValue _nullFallbackValue;

    public NullDelegateMapping(
        ITypeSymbol nullableSourceType,
        ITypeSymbol nullableTargetType,
        TypeMapping delegateMapping,
        NullFallbackValue nullFallbackValue)
        : base(nullableSourceType, nullableTargetType)
    {
        _delegateMapping = delegateMapping;
        _nullFallbackValue = nullFallbackValue;
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
    {
        if (_delegateMapping.SourceType.IsNullable())
            return _delegateMapping.Build(source);

        if (!SourceType.IsNullable())
        {
            // if the target type is a nullable value type, there needs to be an additional cast
            // eg. int => int? needs to be casted.
            return TargetType.IsNullableValueType()
                ? CastExpression(IdentifierName(TargetType.ToDisplayString()), _delegateMapping.Build(source))
                : _delegateMapping.Build(source);
        }

        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        // source == null ? <null-substitute> : Map(source)
        // or for nullable value types:
        // source == null ? <null-substitute> : Map(source.Value)
        var sourceValue = SourceType.IsNullableValueType()
            ? MemberAccess(source, NullableValueProperty)
            : source;

        return ConditionalExpression(
            IsNull(source),
            NullSubstitute(TargetType.NonNullable(), source, _nullFallbackValue),
            _delegateMapping.Build(sourceValue));
    }
}
