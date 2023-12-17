using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ToStringMappingBuilder
{
    public static NewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.ToStringMethod))
            return null;

        if (ctx.Target.SpecialType != SpecialType.System_String)
            return null;

        var (formatProvider, formatProviderIsDefault) = ctx.GetFormatProvider(ctx.MappingKey.Configuration.FormatProviderName);
        var stringFormat = ctx.MappingKey.Configuration.StringFormat;
        if (stringFormat == null && formatProvider == null)
            return new ToStringMapping(ctx.Source, ctx.Target);

        return (stringFormat, formatProvider, formatProviderIsDefault) switch
        {
            // ToString(string, IFormatProvider)
            (not null, not null, _) when HasToStringMethod(ctx, true, true)
                => new ToStringMapping(ctx.Source, ctx.Target, stringFormat, formatProvider.Name),

            // ToString(string)
            (not null, not null, true) when HasToStringMethod(ctx, true, false)
                => new ToStringMapping(ctx.Source, ctx.Target, stringFormat),

            // ToString(string)
            (not null, null, _) when HasToStringMethod(ctx, true, false) => new ToStringMapping(ctx.Source, ctx.Target, stringFormat),

            // ToString(string, null)
            (not null, null, _) when HasToStringMethodWithNullableParameter(ctx, 1)
                => new ToStringMapping(ctx.Source, ctx.Target, stringFormat, simpleInvocation: false),

            // ToString(IFormatProvider)
            (null, not null, _) when HasToStringMethod(ctx, false, true)
                => new ToStringMapping(ctx.Source, ctx.Target, formatProviderName: formatProvider.Name),

            // ToString(null, IFormatProvider)
            (null, not null, _) when HasToStringMethodWithNullableParameter(ctx, 0)
                => new ToStringMapping(ctx.Source, ctx.Target, formatProviderName: formatProvider.Name, simpleInvocation: false),

            // ToString()
            (null, not null, true) => new ToStringMapping(ctx.Source, ctx.Target),

            _ => ReportDiagnosticAndBuildUnformattedMapping(ctx),
        };
    }

    private static ToStringMapping ReportDiagnosticAndBuildUnformattedMapping(MappingBuilderContext ctx)
    {
        ctx.ReportDiagnostic(DiagnosticDescriptors.SourceDoesNotImplementToStringWithFormatParameters, ctx.Source);
        return new ToStringMapping(ctx.Source, ctx.Target);
    }

    private static bool HasToStringMethod(MappingBuilderContext ctx, bool stringFormatParam, bool formatProviderParam) =>
        FindToStringMethod(ctx, stringFormatParam, formatProviderParam) != null;

    private static bool HasToStringMethodWithNullableParameter(MappingBuilderContext ctx, int nullableParameterIndex) =>
        FindToStringMethod(ctx, true, true) is { } m && m.Parameters[nullableParameterIndex].NullableAnnotation.IsNullable();

    private static IMethodSymbol? FindToStringMethod(MappingBuilderContext ctx, bool stringFormatParam, bool formatProviderParam)
    {
        return ctx.SymbolAccessor.GetAllMethods(ctx.Source, nameof(ToString))
            .FirstOrDefault(m => IsToStringMethod(ctx, m, stringFormatParam, formatProviderParam));
    }

    private static bool IsToStringMethod(MappingBuilderContext ctx, IMethodSymbol method, bool stringFormatParam, bool formatProviderParam)
    {
        if (
            method
            is not {
                MethodKind: MethodKind.Ordinary,
                IsAsync: false,
                ReturnType.SpecialType: SpecialType.System_String,
                Parameters.Length: 1 or 2,
                IsGenericMethod: false
            }
        )
        {
            return false;
        }

        return (stringFormatParam, formatProviderParam) switch
        {
            (true, true)
                => method.Parameters.Length == 2
                    && method.Parameters[0].Type.SpecialType == SpecialType.System_String
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, ctx.Types.Get<IFormatProvider>()),
            (true, false) => method.Parameters is [{ Type.SpecialType: SpecialType.System_String }],
            (false, true)
                => method.Parameters.Length == 1
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, ctx.Types.Get<IFormatProvider>()),
            _ => false,
        };
    }
}
