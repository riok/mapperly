using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class ConvertStaticMethodMappingBuilder
{
    public static StaticMethodMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        //checks for backward compatibility

        if (IsDateTimeToDateOnlyConversion(ctx))
        {
            if (!ctx.IsConversionEnabled(MappingConversionType.DateTimeToDateOnly))
                return null;
        }
        else if (IsDateTimeToTimeOnlyConversion(ctx))
        {
            if (!ctx.IsConversionEnabled(MappingConversionType.DateTimeToTimeOnly))
                return null;
        }
        else
        {
            if (!ctx.IsConversionEnabled(MappingConversionType.StaticConvertMethods))
                return null;
        }

        var targetIsNullable = ctx.Target.NonNullable(out var nonNullableTarget);

        var allTargetMethods = ctx.SymbolAccessor.GetAllMethods(nonNullableTarget).ToArray();

        var isTargetMapping = TryGetStaticMethodMapping(
            allTargetMethods,
            GetTargetStaticMethodNames(ctx),
            ctx.Source,
            ctx.Target,
            nonNullableTarget,
            targetIsNullable,
            out var mapping
        );

        if (isTargetMapping)
        {
            return mapping;
        }

        var allSourceMethods = ctx.SymbolAccessor.GetAllMethods(ctx.Source);

        if (ctx.Source.IsArrayType(out var arrayTypeSymbol))
        {
            allSourceMethods = allSourceMethods.Concat(ctx.SymbolAccessor.GetAllMethods(arrayTypeSymbol.ElementType));
        }
        else
        {
            if (ctx.Source is INamedTypeSymbol { TypeArguments.Length: 1 } namedTypeSymbol)
            {
                allSourceMethods = allTargetMethods.Concat(ctx.SymbolAccessor.GetAllMethods(namedTypeSymbol.TypeArguments[0]));
            }
        }

        return TryGetStaticMethodMapping(
            allSourceMethods.ToArray(),
            GetSourceStaticMethodNames(ctx),
            ctx.Source,
            ctx.Target,
            nonNullableTarget,
            targetIsNullable,
            out mapping
        )
            ? mapping
            : null;
    }

    private static bool IsDateTimeToDateOnlyConversion(MappingBuilderContext ctx)
    {
        return ctx.Source.SpecialType == SpecialType.System_DateTime
                && ctx.Types.DateOnly != null
                && ctx.Target is INamedTypeSymbol namedSymbol
                && SymbolEqualityComparer.Default.Equals(namedSymbol, ctx.Types.DateOnly)
            || true;
    }

    private static bool IsDateTimeToTimeOnlyConversion(MappingBuilderContext ctx)
    {
        return ctx.Source.SpecialType == SpecialType.System_DateTime
                && ctx.Types.TimeOnly != null
                && ctx.Target is INamedTypeSymbol namedSymbol
                && SymbolEqualityComparer.Default.Equals(namedSymbol, ctx.Types.TimeOnly)
            || true;
    }

    private static bool TryGetStaticMethodMapping(
        ICollection<IMethodSymbol> allMethods,
        IEnumerable<string> methodNames,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeSymbol nonNullableTargetType,
        bool targetIsNullable,
        out StaticMethodMapping? mapping
    )
    {
        foreach (var methodName in methodNames)
        {
            var candidates = GetMethodCandidates(allMethods, methodName, sourceType);

            // try to find method with equal nullability return type
            var method = candidates.FirstOrDefault(x => SymbolEqualityComparer.IncludeNullability.Equals(x.ReturnType, targetType));

            if (method != null)
            {
                mapping = new StaticMethodMapping(method);
                return true;
            }

            if (!targetIsNullable)
                continue;

            // otherwise try to find method ignoring the nullability
            method = candidates.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.ReturnType, nonNullableTargetType));

            if (method == null)
            {
                continue;
            }

            mapping = new StaticMethodMapping(method);
            return true;
        }

        mapping = null;
        return false;
    }

    private static IMethodSymbol[] GetMethodCandidates(ICollection<IMethodSymbol> allMethods, string methodName, ITypeSymbol parameterType)
    {
        return allMethods
            .Where(m =>
                string.Equals(m.Name, methodName, StringComparison.Ordinal)
                && m is { IsStatic: true, ReturnsVoid: false, IsAsync: false, Parameters.Length: 1 }
                && FilterParameterType(m.Parameters[0], parameterType)
            )
            .ToArray();
    }

    private static bool FilterParameterType(IParameterSymbol parameter, ITypeSymbol targetType)
    {
        if (SymbolEqualityComparer.Default.Equals(parameter.Type, targetType))
            return true;

        if (!parameter.IsParams)
            return false;

        var targetIsArray = targetType.IsArrayType(out var targetArrayTypeSymbol);

        var targetIsEnumerable = targetType.AllInterfaces.Any(x =>
            x.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
        );

        if (parameter.Type.IsArrayType(out var arrayTypeSymbol))
        {
            return SymbolEqualityComparer.Default.Equals(
                arrayTypeSymbol.ElementType,
                targetIsArray ? targetArrayTypeSymbol!.ElementType : targetType
            );
        }

        if (!parameter.IsParamsCollection)
            return false;

        return targetIsEnumerable
            ? parameter.Type.AllInterfaces.Intersect(targetType.AllInterfaces, SymbolEqualityComparer.Default).Any()
            : SymbolEqualityComparer.Default.Equals(((INamedTypeSymbol)parameter.Type)?.TypeArguments[0], targetType);
    }

    private static IEnumerable<string> GetTargetStaticMethodNames(MappingBuilderContext ctx)
    {
        const string create = "Create";
        const string from = "From";

        yield return create;
        yield return $"{create}{from}";

        if (!ctx.Source.IsArrayType(out var arrayType))
            yield return $"{from}{ctx.Source.Name}";
        else
            yield return $"{from}{arrayType.ElementType.Name}Array";
    }

    private static IEnumerable<string> GetSourceStaticMethodNames(MappingBuilderContext ctx)
    {
        var nonNullableTarget = ctx.Target.NonNullable();

        yield return $"To{nonNullableTarget.Name}";

        if (!nonNullableTarget.IsArrayType(out var arrayTypeSymbol))
            yield break;

        var nonNullableElementType = arrayTypeSymbol.ElementType.NonNullable();

        yield return $"To{nonNullableElementType.Name}Array";
    }
}
