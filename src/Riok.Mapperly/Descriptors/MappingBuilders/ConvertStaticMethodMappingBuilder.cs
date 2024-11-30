using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ConvertStaticMethodMappingBuilder
{
    public static StaticMethodMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!IsConversionEnabled(ctx))
            return null;

        var targetIsNullable = ctx.Target.NonNullable(out var nonNullableTarget);

        var allTargetMethods = ctx.SymbolAccessor.GetAllMethods(nonNullableTarget).ToList();

        var mapping = TryGetStaticMethodMapping(
            ctx.SymbolAccessor,
            allTargetMethods,
            GetTargetStaticMethodNames(ctx),
            ctx.Source,
            ctx.Target,
            nonNullableTarget,
            targetIsNullable
        );

        if (mapping is not null)
        {
            return mapping;
        }

        var allSourceMethods = ctx.SymbolAccessor.GetAllMethods(ctx.Source);

        // collect also methods from source type generic argument, for example `TTarget ToTarget(List<A> source)`
        if (ctx.Source is INamedTypeSymbol { TypeArguments.Length: 1 } namedTypeSymbol)
        {
            allSourceMethods = allSourceMethods.Concat(ctx.SymbolAccessor.GetAllMethods(namedTypeSymbol.TypeArguments[0]));
        }

        return TryGetStaticMethodMapping(
            ctx.SymbolAccessor,
            allSourceMethods.ToList(),
            GetSourceStaticMethodNames(ctx),
            ctx.Source,
            ctx.Target,
            nonNullableTarget,
            targetIsNullable
        );
    }

    private static bool IsConversionEnabled(MappingBuilderContext ctx)
    {
        //checks for backward compatibility
        if (IsDateTimeToDateOnlyConversion(ctx))
        {
            if (!ctx.IsConversionEnabled(MappingConversionType.DateTimeToDateOnly))
                return false;
        }
        else if (IsDateTimeToTimeOnlyConversion(ctx))
        {
            if (!ctx.IsConversionEnabled(MappingConversionType.DateTimeToTimeOnly))
                return false;
        }
        else if (!ctx.IsConversionEnabled(MappingConversionType.StaticConvertMethods))
            return false;

        return true;
    }

    private static bool IsDateTimeToDateOnlyConversion(MappingBuilderContext ctx)
    {
        return ctx.Source.SpecialType == SpecialType.System_DateTime
            && ctx.Types.DateOnly != null
            && ctx.Target is INamedTypeSymbol namedSymbol
            && SymbolEqualityComparer.Default.Equals(namedSymbol, ctx.Types.DateOnly);
    }

    private static bool IsDateTimeToTimeOnlyConversion(MappingBuilderContext ctx)
    {
        return ctx.Source.SpecialType == SpecialType.System_DateTime
            && ctx.Types.TimeOnly != null
            && ctx.Target is INamedTypeSymbol namedSymbol
            && SymbolEqualityComparer.Default.Equals(namedSymbol, ctx.Types.TimeOnly);
    }

    private static StaticMethodMapping? TryGetStaticMethodMapping(
        SymbolAccessor symbolAccessor,
        List<IMethodSymbol> allMethods,
        IEnumerable<string> methodNames,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeSymbol nonNullableTargetType,
        bool targetIsNullable
    )
    {
        // Get all methods with a single parameter whose type is suitable for assignment to the source type,
        // group them by name,
        // and convert them to a dictionary whose key is the method name.
        // The keys in the dictionary are compared case-insensitively to handle possible `Uint` vs `UInt` cases, etc.
        var allMethodCandidates = allMethods
            .Where(m => m is { IsStatic: true, ReturnsVoid: false, IsAsync: false, Parameters.Length: 1 })
            .GroupBy(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var methodName in methodNames)
        {
            if (!allMethodCandidates.TryGetValue(methodName, out var candidates))
                continue;

            if (!targetIsNullable)
            {
                var method = candidates.Find(x => symbolAccessor.ValidateSignature(x, nonNullableTargetType, sourceType));
                if (method != null)
                    return new StaticMethodMapping(method);
            }
            else // currently this branch is never executed because `targetType` is never null. See https://github.com/riok/mapperly/issues/1614
            {
                var method = candidates.Find(x => symbolAccessor.ValidateSignature(x, targetType, sourceType));
                if (method != null)
                    return new StaticMethodMapping(method);
            }
        }

        return null;
    }

    private static IEnumerable<string> GetTargetStaticMethodNames(MappingBuilderContext ctx)
    {
        const string create = "Create";
        const string from = "From";
        const string array = "Array";

        if (ctx.Source.IsArrayType(out var arrayType))
        {
            yield return $"{create}{from}{arrayType.ElementType.Name}{array}";
            yield return $"{from}{arrayType.ElementType.Name}{array}";

            if (!arrayType.ElementType.HasKeyword(out var keywordName))
                yield break;

            yield return $"{create}{from}{keywordName}{array}";

            yield return $"{from}{keywordName}{array}";

            yield return $"{from}{array}";
            yield return $"{create}{from}{array}";
            yield return $"{create}{from}";
            yield return create;
        }
        else
        {
            yield return $"{create}{from}{ctx.Source.Name}";
            yield return $"{from}{ctx.Source.Name}";

            if (ctx.Source.HasKeyword(out var sourceKeyword))
            {
                yield return $"{create}{from}{sourceKeyword}";
                yield return $"{from}{sourceKeyword}";
            }

            yield return $"{create}{from}";
            yield return create;
        }
    }

    private static IEnumerable<string> GetSourceStaticMethodNames(MappingBuilderContext ctx)
    {
        var nonNullableTarget = ctx.Target.NonNullable();

        yield return $"To{nonNullableTarget.Name}";

        if (nonNullableTarget.HasKeyword(out var keywordName))
            yield return $"To{keywordName}";

        if (!nonNullableTarget.IsArrayType(out var arrayTypeSymbol))
            yield break;

        var nonNullableElementType = arrayTypeSymbol.ElementType.NonNullable();

        yield return $"To{nonNullableElementType.Name}Array";

        if (nonNullableElementType.HasKeyword(out var elementTypeName))
            yield return $"To{elementTypeName}Array";
    }
}
