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
/// <remarks>
/// This mapping handles null propagation by wrapping a delegate mapping with appropriate
/// null checks and fallback values. It optimizes away when no null handling is required.
/// </remarks>
public class NullDelegateMapping(
    ITypeSymbol nullableSourceType,
    ITypeSymbol nullableTargetType,
    INewInstanceMapping delegateMapping,
    NullFallbackValue nullFallbackValue
) : NewInstanceMapping(nullableSourceType, nullableTargetType)
{
    private const string NullableValueProperty = nameof(Nullable<>.Value);

    /// <summary>
    /// Indicates whether this mapping produces no code (is synthetic).
    /// </summary>
    /// <remarks>
    /// The mapping is synthetic when:
    /// <list type="bullet">
    ///   <item>The delegate mapping is synthetic AND</item>
    ///   <item>No null handling is required (delegate accepts nulls, source isn't nullable, or both types are nullable)</item>
    /// </list>
    /// </remarks>
    public override bool IsSynthetic { get; } = ComputeIsSynthetic(delegateMapping, nullableSourceType, nullableTargetType);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // Fast paths: delegate handles nulls or no transformation needed
        if (delegateMapping.SourceType.IsNullable() || IsSynthetic)
        {
            return delegateMapping.Build(ctx);
        }

        // Non-nullable source: may need cast for nullable value type targets
        if (!SourceType.IsNullable())
        {
            return BuildNonNullableSourceMapping(ctx);
        }

        // Nullable source with delegate that can't handle nulls: add null check
        return BuildNullableSourceMapping(ctx);
    }

    private static bool ComputeIsSynthetic(INewInstanceMapping delegateMapping, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        if (!delegateMapping.IsSynthetic)
            return false;

        // No null handling needed if:
        // 1. Delegate already accepts nulls
        // 2. Source isn't nullable and target isn't nullable value type (no conversion needed)
        // 3. Both source and target are nullable (pass-through)
        var delegateAcceptsNulls = delegateMapping.SourceType.IsNullable();
        var noConversionNeeded = !sourceType.IsNullable() && !targetType.IsNullableValueType();
        var bothNullable = sourceType.IsNullable() && targetType.IsNullable();

        return delegateAcceptsNulls || noConversionNeeded || bothNullable;
    }

    private ExpressionSyntax BuildNonNullableSourceMapping(TypeMappingBuildContext ctx)
    {
        var mappedExpression = delegateMapping.Build(ctx);

        // Cast required for nullable value type targets (e.g., int => int? in LINQ)
        return TargetType.IsNullableValueType() ? CastExpression(FullyQualifiedIdentifier(TargetType), mappedExpression) : mappedExpression;
    }

    private ExpressionSyntax BuildNullableSourceMapping(TypeMappingBuildContext ctx)
    {
        var nullSubstitute = NullSubstitute(TargetType, ctx.Source, nullFallbackValue);

        // Optimization: use coalesce operator for throw scenarios with simple mappings
        // Generates: Map(source ?? throw new ArgumentNullException(...))
        if (CanUseCoalesceOptimization())
        {
            var coalesceCtx = ctx.WithSource(Coalesce(ctx.Source, nullSubstitute));
            return delegateMapping.Build(coalesceCtx);
        }

        // Standard path: conditional expression
        // Generates: source == null ? <fallback> : Map(source.Value)
        var nonNullableSource = GetNonNullableSourceExpression(ctx.Source);
        var mappedExpression = delegateMapping.Build(ctx.WithSource(nonNullableSource));

        return Conditional(IsNull(ctx.Source), nullSubstitute, mappedExpression);
    }

    private bool CanUseCoalesceOptimization() =>
        nullFallbackValue == NullFallbackValue.ThrowArgumentNullException
        && (delegateMapping.IsSynthetic || delegateMapping is MethodMapping);

    private ExpressionSyntax GetNonNullableSourceExpression(ExpressionSyntax source)
    {
        var result = source;

        // Suppress nullable warning for array element access
        if (result is ElementAccessExpressionSyntax)
        {
            result = PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, result);
        }

        // Access .Value for nullable value types
        if (SourceType.IsNullableValueType())
        {
            result = MemberAccess(result, NullableValueProperty);
        }

        return result;
    }
}
