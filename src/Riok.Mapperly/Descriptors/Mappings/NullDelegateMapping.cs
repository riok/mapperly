using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Null aware delegate mapping. Abstracts handling null values of the delegated mapping.
/// </summary>
public class NullDelegateMapping : NewInstanceMapping
{
    private const string NullableValueProperty = nameof(Nullable<>.Value);

    private readonly INewInstanceMapping _delegateMapping;
    private readonly NullFallbackValue _nullFallbackValue;

    public NullDelegateMapping(
        ITypeSymbol nullableSourceType,
        ITypeSymbol nullableTargetType,
        INewInstanceMapping delegateMapping,
        NullFallbackValue nullFallbackValue
    )
        : base(nullableSourceType, nullableTargetType)
    {
        _delegateMapping = delegateMapping;
        _nullFallbackValue = nullFallbackValue;

        // the mapping is synthetic (produces no code)
        // if and only if the delegate mapping is synthetic (produces also no code)
        // and no null handling is required
        // (this is the case if the delegate mapping source type accepts nulls
        // or the source type is not nullable and the target type is not a nullable value type (otherwise a conversion is needed)).
        IsSynthetic =
            _delegateMapping.IsSynthetic
            && (_delegateMapping.SourceType.IsNullable() || !SourceType.IsNullable() && !TargetType.IsNullableValueType());
    }

    public override bool IsSynthetic { get; }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        if (_delegateMapping.SourceType.IsNullable())
            return _delegateMapping.Build(ctx);

        if (!SourceType.IsNullable())
        {
            // if the target type is a nullable value type, there needs to be an additional cast in some cases
            // (e.g. in a linq expression, int => int?)
            return TargetType.IsNullableValueType()
                ? CastExpression(FullyQualifiedIdentifier(TargetType), _delegateMapping.Build(ctx))
                : _delegateMapping.Build(ctx);
        }

        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        // with coalesce: Map(source ?? throw)
        // or with if-else:
        // source == null ? <null-substitute> : Map(source)
        // or for nullable value types:
        // source == null ? <null-substitute> : Map(source.Value)
        var nullSubstitute = NullSubstitute(TargetType, ctx.Source, _nullFallbackValue);

        // if throw is used instead of a default value
        // and the delegate mapping is a synthetic or method mapping
        // the coalesce operator can be used
        // (the Map method isn't invoked in the null case since the exception is thrown,
        // and it is ensured no parentheses are needed since it is directly used or is passed as argument).
        // This simplifies the generated source code.
        if (
            _nullFallbackValue == NullFallbackValue.ThrowArgumentNullException
            && (_delegateMapping.IsSynthetic || _delegateMapping is MethodMapping)
        )
        {
            ctx = ctx.WithSource(Coalesce(ctx.Source, nullSubstitute));
            return _delegateMapping.Build(ctx);
        }

        var nonNullableSourceValue = ctx.Source;

        // disable nullable waring if accessing array
        if (nonNullableSourceValue is ElementAccessExpressionSyntax)
        {
            nonNullableSourceValue = PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, nonNullableSourceValue);
        }

        // if it is a value type, access the value property
        if (SourceType.IsNullableValueType())
        {
            nonNullableSourceValue = MemberAccess(nonNullableSourceValue, NullableValueProperty);
        }

        return Conditional(IsNull(ctx.Source), nullSubstitute, _delegateMapping.Build(ctx.WithSource(nonNullableSourceValue)));
    }
}
