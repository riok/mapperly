using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A delegate mapping which intentionally skips any null handling of the delegated mapping.
/// The source is assumed to be non-<c>null</c> and is mapped directly without a null guard.
/// This is used for queryable projections / expression mappings configured with
/// <see cref="Abstractions.QueryableProjectionNullHandling.Ignore"/>, where the projection is
/// translated to a query (e.g. by EF Core) and the null guard would only produce unwanted
/// <c>CASE</c> expressions.
/// Unlike <see cref="NullDelegateMapping"/> it never emits a <c>source == null ? ...</c> branch,
/// but it still overwrites the (nullable) source/target types and unwraps nullable value types
/// so the delegated mapping receives a non-nullable value.
/// </summary>
public class NullBypassDelegateMapping(ITypeSymbol nullableSourceType, ITypeSymbol nullableTargetType, INewInstanceMapping delegateMapping)
    : NewInstanceMapping(nullableSourceType, nullableTargetType)
{
    private const string NullableValueProperty = nameof(Nullable<>.Value);

    // the mapping is synthetic (produces no code)
    // if and only if the delegate mapping is synthetic (produces also no code)
    // and no type conversion is required
    // (this is the case if the delegate mapping source type accepts nulls
    // or neither the source nor the target type is a nullable value type (otherwise a conversion is needed)).
    public override bool IsSynthetic =>
        delegateMapping.IsSynthetic
        && (delegateMapping.SourceType.IsNullable() || !SourceType.IsNullableValueType() && !TargetType.IsNullableValueType());

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // the delegate mapping handles nulls itself, nothing to do.
        if (delegateMapping.SourceType.IsNullable())
            return delegateMapping.Build(ctx);

        var sourceValue = ctx.Source;

        // for nullable value types access the underlying value,
        // so the delegate mapping receives a non-nullable value.
        if (SourceType.IsNullableValueType())
        {
            // disable nullable warning if accessing an array element
            if (sourceValue is ElementAccessExpressionSyntax)
            {
                sourceValue = PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, sourceValue);
            }

            sourceValue = MemberAccess(sourceValue, NullableValueProperty);
        }

        var mapped = delegateMapping.Build(ctx.WithSource(sourceValue));

        // if the target type is a nullable value type, there needs to be an additional cast in some cases
        // (e.g. in a linq expression, int => int?)
        return TargetType.IsNullableValueType() ? CastExpression(FullyQualifiedIdentifier(TargetType), mapped) : mapped;
    }
}
