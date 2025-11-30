using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ParseMappingBuilder
{
    private const string ParseMethodName = "Parse";

    public static ParseMethodMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.ParseMethod))
            return null;

        if (ctx.Source.SpecialType != SpecialType.System_String)
            return null;

        var (formatProvider, formatProviderIsDefault) = ctx.GetFormatProvider(ctx.MappingKey.Configuration.FormatProviderName);

        return (formatProvider, formatProviderIsDefault) switch
        {
            // Parse(string, IFormatProvider)
            (not null, _) when FindParseMethod(ctx, true) is { } parseMethod => new ParseMethodMapping(parseMethod, formatProvider.Name),

            // Parse(string)
            (not null, true) when FindParseMethod(ctx, false) is { } parseMethod => new ParseMethodMapping(parseMethod),

            // Parse(string)
            (null, _) when FindParseMethod(ctx, false) is { } parseMethod => new ParseMethodMapping(parseMethod),

            // Parse(string, null)
            (null, _) when FindParseMethodWithNullableParameter(ctx, 1) is { } parseMethod => new ParseMethodMapping(
                parseMethod,
                simpleInvocation: false
            ),

            _ => null,
        };
    }

    private static IMethodSymbol? FindParseMethodWithNullableParameter(MappingBuilderContext ctx, int nullableParameterIndex)
    {
        return FindParseMethod(ctx, true) is { } m && m.Parameters[nullableParameterIndex].NullableAnnotation.IsNullable() ? m : null;
    }

    private static IMethodSymbol? FindParseMethod(MappingBuilderContext ctx, bool formatProviderParam)
    {
        var targetIsNullable = ctx.Target.NonNullable(out var nonNullableTarget);
        return ctx
            .SymbolAccessor.GetAllMethods(nonNullableTarget, ParseMethodName)
            .FirstOrDefault(m =>
                IsParseMethod(ctx, m, formatProviderParam)
                && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, ctx.Source)
                && !ctx.SymbolAccessor.HasAttribute<MapperIgnoreAttribute>(m)
            );
    }

    private static bool IsParseMethod(MappingBuilderContext ctx, IMethodSymbol method, bool formatProviderParam)
    {
        if (method is not { IsStatic: true, ReturnsVoid: false, IsAsync: false, Parameters.Length: 1 or 2, IsGenericMethod: false })
        {
            return false;
        }

        return formatProviderParam switch
        {
            true => method.Parameters.Length == 2
                && method.Parameters[0].Type.SpecialType == SpecialType.System_String
                && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, ctx.Types.Get<IFormatProvider>()),
            false => method.Parameters.Length == 1 && method.Parameters[0].Type.SpecialType == SpecialType.System_String,
        };
    }
}
