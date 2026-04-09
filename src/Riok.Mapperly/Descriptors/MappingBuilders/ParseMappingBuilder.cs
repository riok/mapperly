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
        ctx.Target.NonNullable(out var nonNullableTarget);
        var parseMethods = ctx
            .SymbolAccessor.GetAllMethods(nonNullableTarget, ParseMethodName)
            .Where(m => !MapperIgnoreHelper.CheckIgnored(m, m.Name, ctx))
            .ToList();

        return (formatProvider, formatProviderIsDefault) switch
        {
            // Parse(string, IFormatProvider)
            (not null, _) when FindParseMethod(ctx, parseMethods, true) is { } parseMethod => new ParseMethodMapping(
                parseMethod,
                formatProvider.Name
            ),

            // Parse(string)
            (not null, true) when FindParseMethod(ctx, parseMethods, false) is { } parseMethod => new ParseMethodMapping(parseMethod),

            // Parse(string)
            (null, _) when FindParseMethod(ctx, parseMethods, false) is { } parseMethod => new ParseMethodMapping(parseMethod),

            // Parse(string, null)
            (null, _) when FindParseMethodWithNullableParameter(ctx, parseMethods) is { } parseMethod => new ParseMethodMapping(
                parseMethod,
                simpleInvocation: false
            ),

            _ => null,
        };
    }

    private static IMethodSymbol? FindParseMethodWithNullableParameter(
        MappingBuilderContext ctx,
        IReadOnlyCollection<IMethodSymbol> parseMethods
    )
    {
        return FindParseMethod(ctx, parseMethods, true) is { } m && m.Parameters[1].NullableAnnotation.IsNullable() ? m : null;
    }

    private static IMethodSymbol? FindParseMethod(
        MappingBuilderContext ctx,
        IEnumerable<IMethodSymbol> parseMethods,
        bool formatProviderParam
    )
    {
        return parseMethods.FirstOrDefault(m => IsParseMethod(ctx, m, formatProviderParam));

        static bool IsParseMethod(MappingBuilderContext ctx, IMethodSymbol method, bool formatProviderParam)
        {
            if (method is not { IsStatic: true, ReturnsVoid: false, IsAsync: false, Parameters.Length: 1 or 2, IsGenericMethod: false })
            {
                return false;
            }

            if (!SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, ctx.Source))
            {
                return false;
            }

            return formatProviderParam switch
            {
                true => method.Parameters.Length == 2
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, ctx.Types.Get<IFormatProvider>()),
                false => method.Parameters.Length == 1,
            };
        }
    }
}
