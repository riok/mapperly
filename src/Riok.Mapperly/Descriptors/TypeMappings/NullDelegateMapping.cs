using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Null aware delegate mapping. Abstracts handling null values of the delegated mapping.
/// </summary>
public class NullDelegateMapping : TypeMapping
{
    private const string NullableValueProperty = "Value";

    private readonly bool _targetIsNullable;
    private readonly bool _sourceIsNullable;
    private readonly TypeMapping _delegateMapping;

    public NullDelegateMapping(ITypeSymbol nullableSourceType, ITypeSymbol nullableTargetType, TypeMapping delegateMapping)
        : base(nullableSourceType, nullableTargetType)
    {
        _sourceIsNullable = nullableSourceType.IsNullable();
        _targetIsNullable = nullableTargetType.IsNullable();
        _delegateMapping = delegateMapping;
    }

    public NullDelegateMapping(
        bool sourceIsNullable,
        bool targetIsNullable,
        ITypeSymbol nullableSourceType,
        ITypeSymbol nullableTargetType,
        TypeMapping delegateMapping)
        : base(nullableSourceType, nullableTargetType)
    {
        _sourceIsNullable = sourceIsNullable;
        _targetIsNullable = targetIsNullable;
        _delegateMapping = delegateMapping;
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
    {
        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        if (_sourceIsNullable && !_delegateMapping.SourceType.IsNullable())
        {
            // for direct assignments
            // source ?? <null-substitute>;
            if (_delegateMapping is DirectAssignmentMapping)
            {
                return _targetIsNullable
                    ? _delegateMapping.Build(source)
                    : Coalesce(
                        _delegateMapping.Build(source),
                        NullSubstitute(TargetType.NonNullable(), source));
            }

            // for non direct assignments
            // source == null ? <null-substitute> : Map(source)
            // or for nullable value types:
            // source == null ? <null-substitute> : Map(source.Value)
            var sourceValue = SourceType.IsNullableValueType()
                ? MemberAccess(source, NullableValueProperty)
                : source;

            return ConditionalExpression(
                IsNull(source),
                _targetIsNullable ? DefaultLiteral() : NullSubstitute(TargetType.NonNullable(), source),
                _delegateMapping.Build(sourceValue));
        }

        // target can not be nullable and the map method may return null values
        // therefore we replace Map(source) with Map(source) ?? <null-substitute>;
        if (!_targetIsNullable && _delegateMapping.TargetType.IsNullable())
        {
            return Coalesce(
                _delegateMapping.Build(source),
                NullSubstitute(TargetType.NonNullable(), source));
        }

        return _delegateMapping.Build(source);
    }
}
