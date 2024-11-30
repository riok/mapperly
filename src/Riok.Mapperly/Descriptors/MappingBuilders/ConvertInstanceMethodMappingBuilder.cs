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

        var methodName = GetMappingMethodName(ctx);

        var methodCandidates = ctx
            .SymbolAccessor.GetAllMethods(ctx.Source, methodName)
            .Where(x => x is { IsStatic: false, ReturnsVoid: false, IsAsync: false, Parameters.Length: 0 })
            .ToArray();

        // try to find method with equal nullability return type
        var method = methodCandidates.FirstOrDefault(x => SymbolEqualityComparer.IncludeNullability.Equals(x.ReturnType, ctx.Target));
        if (method is not null)
            return new SourceObjectMethodMapping(ctx.Source, ctx.Target, method.Name);

        if (!targetIsNullable)
            return null;

        // otherwise try to find method ignoring the nullability
        method = methodCandidates.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.ReturnType, nonNullableTarget));

        return method is null ? null : new SourceObjectMethodMapping(ctx.Source, ctx.Target, method.Name);
    }

    private static string GetMappingMethodName(MappingBuilderContext ctx)
    {
        var nonNullableTarget = ctx.Target.NonNullable();

        if (!nonNullableTarget.IsArrayType(out var arrayType))
        {
            return $"To{nonNullableTarget.Name}";
        }

        var nonNullableElementType = arrayType.ElementType.NonNullable();

        return $"To{nonNullableElementType.Name}Array";
    }
}
