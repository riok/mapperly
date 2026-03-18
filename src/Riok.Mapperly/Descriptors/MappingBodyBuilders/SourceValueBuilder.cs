using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

internal static class SourceValueBuilder
{
    /// <summary>
    /// Tries to build an <see cref="ISourceValue"/> instance which serializes as an expression,
    /// with a value that is assignable to the target member requested in the <paramref name="memberMappingInfo"/>.
    /// </summary>
    public static bool TryBuildMappedSourceValue(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberMappingInfo,
        [NotNullWhen(true)] out ISourceValue? sourceValue
    ) => TryBuildMappedSourceValue(ctx, memberMappingInfo, MemberMappingBuilder.CodeStyle.Expression, out sourceValue);

    /// <summary>
    /// Tries to build an <see cref="ISourceValue"/> instance,
    /// with a value that is assignable to the target member requested in the <paramref name="memberMappingInfo"/>.
    /// </summary>
    public static bool TryBuildMappedSourceValue(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberMappingInfo,
        MemberMappingBuilder.CodeStyle codeStyle,
        [NotNullWhen(true)] out ISourceValue? sourceValue
    )
    {
        if (memberMappingInfo.ValueConfiguration != null)
            return TryBuildValue(ctx, memberMappingInfo, out sourceValue);

        if (memberMappingInfo.SourceMember != null)
            return MemberMappingBuilder.TryBuild(ctx, memberMappingInfo, codeStyle, out sourceValue);

        sourceValue = null;
        return false;
    }

    private static bool TryBuildValue(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberMappingInfo,
        [NotNullWhen(true)] out ISourceValue? sourceValue
    )
    {
        // always set the member mapped,
        // as other diagnostics are reported if the mapping fails to be built
        ctx.SetMembersMapped(memberMappingInfo);

        if (memberMappingInfo.ValueConfiguration!.Value != null)
            return TryBuildConstantSourceValue(ctx, memberMappingInfo, out sourceValue);

        if (memberMappingInfo.ValueConfiguration!.Use != null)
            return TryBuildMethodProvidedSourceValue(ctx, memberMappingInfo, out sourceValue);

        throw new InvalidOperationException($"Illegal {nameof(MemberValueMappingConfiguration)}");
    }

    private static bool TryBuildConstantSourceValue(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberMappingInfo,
        [NotNullWhen(true)] out ISourceValue? sourceValue
    )
    {
        var value = memberMappingInfo.ValueConfiguration!.Value!.Value;

        // the target is a non-nullable reference type,
        // but the provided value is null or default (for default IsNullable is also true)
        if (
            value.ConstantValue.IsNull
            && memberMappingInfo.TargetMember.MemberType.IsReferenceType
            && !memberMappingInfo.TargetMember.Member.IsNullable
        )
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapValueNullToNonNullable,
                memberMappingInfo.TargetMember.ToDisplayString()
            );
            sourceValue = new ConstantSourceValue(SuppressNullableWarning(value.Expression));
            return true;
        }

        // target is value type but value is null
        if (
            value.ConstantValue.IsNull
            && memberMappingInfo.TargetMember.MemberType.IsValueType
            && !memberMappingInfo.TargetMember.MemberType.IsNullableValueType()
            && value.Expression.IsKind(SyntaxKind.NullLiteralExpression)
        )
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.CannotMapValueNullToNonNullable,
                memberMappingInfo.TargetMember.ToDisplayString()
            );
            sourceValue = new ConstantSourceValue(DefaultLiteral());
            return true;
        }

        // the target accepts null and the value is null or default
        // use the expression instant of a constant null literal
        // to use "default" or "null" depending on what the user specified in the attribute
        if (value.ConstantValue.IsNull)
        {
            sourceValue = new ConstantSourceValue(value.Expression);
            return true;
        }

        // use non-nullable target type to allow non-null value type assignments
        // to nullable value types
        if (!SymbolEqualityComparer.Default.Equals(value.ConstantValue.Type, memberMappingInfo.TargetMember.MemberType.NonNullable()))
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.MapValueTypeMismatch,
                value.Expression.ToFullString(),
                value.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                memberMappingInfo.TargetMember.ToDisplayString()
            );
            sourceValue = null;
            return false;
        }

        switch (value.ConstantValue.Kind)
        {
            case TypedConstantKind.Primitive:
                sourceValue = new ConstantSourceValue(value.Expression);
                return true;
            case TypedConstantKind.Enum:
                // expand enum member access to fully qualified identifier
                // use simple member name approach instead of slower visitor pattern on the expression
                var enumMemberName = ((MemberAccessExpressionSyntax)value.Expression).Name.Identifier.Text;
                var enumTypeFullName = FullyQualifiedIdentifier(memberMappingInfo.TargetMember.MemberType.NonNullable());
                sourceValue = new ConstantSourceValue(MemberAccess(enumTypeFullName, enumMemberName));
                return true;
            case TypedConstantKind.Type:
            case TypedConstantKind.Array:
                ctx.BuilderContext.ReportDiagnostic(DiagnosticDescriptors.MapValueUnsupportedType, value.ConstantValue.Kind.ToString());
                break;
        }

        sourceValue = null;
        return false;
    }

    private static bool TryBuildMethodProvidedSourceValue(
        IMembersBuilderContext<IMapping> ctx,
        MemberMappingInfo memberMappingInfo,
        [NotNullWhen(true)] out ISourceValue? sourceValue
    )
    {
        var methodReferenceConfiguration = memberMappingInfo.ValueConfiguration!.Use!;
        var targetSymbol = methodReferenceConfiguration.GetTargetType(ctx.BuilderContext);
        var scope = ctx.BuilderContext.ParameterScope;
        var namedMethodCandidates = targetSymbol is null
            ? []
            : ctx
                .BuilderContext.SymbolAccessor.GetAllDirectlyAccessibleMethods(targetSymbol)
                .Where(m =>
                    m is { IsAsync: false, ReturnsVoid: false, IsGenericMethod: false }
                    && ParameterScope.CanSatisfyParameters(scope, m)
                    && ctx.BuilderContext.AttributeAccessor.IsMappingNameEqualTo(m, methodReferenceConfiguration.Name)
                )
                .ToList();

        if (namedMethodCandidates.Count == 0)
        {
            ReportMethodNotFoundDiagnostic(ctx.BuilderContext, targetSymbol, methodReferenceConfiguration);
            sourceValue = null;
            return false;
        }

        // use non-nullable to allow non-null value type assignments
        // to nullable value types
        // nullable is checked with nullable annotation
        var methodCandidates = namedMethodCandidates.Where(x =>
            SymbolEqualityComparer.Default.Equals(x.ReturnType.NonNullable(), memberMappingInfo.TargetMember.MemberType.NonNullable())
        );

        if (!memberMappingInfo.TargetMember.Member.IsNullable)
        {
            // Filter out methods that may return null when the target is non-nullable.
            methodCandidates = methodCandidates.Where(m => !ctx.BuilderContext.SymbolAccessor.MayReturnNull(m, false));
        }

        var methodSymbol = methodCandidates.FirstOrDefault();
        if (methodSymbol == null)
        {
            ctx.BuilderContext.ReportDiagnostic(
                DiagnosticDescriptors.MapValueMethodTypeMismatch,
                methodReferenceConfiguration.Name,
                namedMethodCandidates[0].ReturnType.ToDisplayString(),
                memberMappingInfo.TargetMember.ToDisplayString()
            );
            sourceValue = null;
            return false;
        }

        // Collect additional parameter names and mark them as used
        var additionalParameterNames = methodSymbol.Parameters.Select(param => param.Name).ToList();

        foreach (var additionalParameterName in additionalParameterNames)
        {
            scope?.MarkUsed(additionalParameterName);
        }

        sourceValue = new MethodProvidedSourceValue(
            methodSymbol.Name,
            methodReferenceConfiguration.GetTargetName(ctx.BuilderContext),
            additionalParameterNames
        );
        return true;
    }

    private static void ReportMethodNotFoundDiagnostic(
        MappingBuilderContext builderCtx,
        ITypeSymbol? targetSymbol,
        IMethodReferenceConfiguration methodRef
    )
    {
        // Check if a method by name exists but with unsatisfiable parameters
        if (
            targetSymbol is not null
            && builderCtx
                .SymbolAccessor.GetAllDirectlyAccessibleMethods(targetSymbol)
                .Any(m =>
                    m is { IsAsync: false, ReturnsVoid: false, IsGenericMethod: false, Parameters.Length: > 0 }
                    && builderCtx.AttributeAccessor.IsMappingNameEqualTo(m, methodRef.Name)
                )
        )
        {
            builderCtx.ReportDiagnostic(DiagnosticDescriptors.MapValueMethodParametersUnsatisfied, methodRef.FullName);
        }
        else
        {
            builderCtx.ReportDiagnostic(DiagnosticDescriptors.MapValueReferencedMethodNotFound, methodRef.FullName);
        }
    }
}
