using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ConvertInstanceMethodMappingBuilder
{
    public static SourceObjectMethodMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.ToTargetMethod))
            return null;

        var targetIsNullable = ctx.Target.NonNullable(out var nonNullableTarget);

        // ignore `ToString` mapping for backward compatibility
        if (nonNullableTarget.SpecialType == SpecialType.System_String)
            return null;

        foreach (var methodName in GetMappingMethodNames(ctx))
        {
            var methodCandidates = ctx
                .SymbolAccessor.GetAllMethods(ctx.Source)
                .Where(m =>
                    string.Equals(m.Name, methodName, StringComparison.OrdinalIgnoreCase)
                    && m is { IsStatic: false, ReturnsVoid: false, IsAsync: false, Parameters.Length: 0 }
                    && !ctx.SymbolAccessor.HasAttribute<MapperIgnoreAttribute>(m)
                )
                .ToList();

            // try to find method with equal nullability return type
            var method = methodCandidates.Find(x => SymbolEqualityComparer.IncludeNullability.Equals(x.ReturnType, ctx.Target));
            if (method is not null)
                return new SourceObjectMethodMapping(ctx.Source, ctx.Target, method.Name);

            if (!targetIsNullable)
                continue;

            // otherwise try to find method ignoring the nullability
            method = methodCandidates.Find(x => SymbolEqualityComparer.Default.Equals(x.ReturnType, nonNullableTarget));

            if (method is null)
                continue;

            return new SourceObjectMethodMapping(ctx.Source, ctx.Target, method.Name);
        }

        return null;
    }

    private static IEnumerable<string> GetMappingMethodNames(MappingBuilderContext ctx)
    {
        var nonNullableTarget = ctx.Target.NonNullable();
        var hasKeyword = nonNullableTarget.HasKeyword(out var keywordName);
        if (!nonNullableTarget.IsArrayType(out var arrayType))
        {
            var methodName = $"To{nonNullableTarget.Name}";
            return hasKeyword ? [methodName, $"To{keywordName}"] : [methodName];
        }

        var nonNullableElementType = arrayType.ElementType.NonNullable();

        var arrayMethodName = $"To{nonNullableElementType.Name}Array";
        return hasKeyword ? [arrayMethodName, $"To{keywordName}Array"] : [arrayMethodName];
    }
}
