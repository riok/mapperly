using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class StringToEnumMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.StringToEnum))
            return null;

        if (ctx.Source.SpecialType != SpecialType.System_String || !ctx.Target.IsEnum())
            return null;

        var genericEnumParseMethodSupported = ctx
            .Types.Get<Enum>()
            .GetMembers(nameof(Enum.Parse))
            .OfType<IMethodSymbol>()
            .Any(x => x.IsGenericMethod);

        if (ctx.IsExpression)
        {
            return new EnumFromStringParseMapping(
                ctx.Source,
                ctx.Target,
                genericEnumParseMethodSupported,
                ctx.Configuration.Enum.IgnoreCase
            );
        }

        return BuildEnumFromStringSwitchMapping(ctx, genericEnumParseMethodSupported);
    }

    private static EnumFromStringSwitchMapping BuildEnumFromStringSwitchMapping(
        MappingBuilderContext ctx,
        bool genericEnumParseMethodSupported
    )
    {
        var ignoredTargetMembers = ctx.Configuration.Enum.IgnoredTargetMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        var explicitMappings = BuildExplicitValueMappings(ctx);

        // from string => use an optimized method of Enum.Parse which would use slow reflection
        // however we currently don't support all features of Enum.Parse yet (ex. flags)
        // therefore we use Enum.Parse as fallback.
        var fallbackMapping = BuildFallbackParseMapping(ctx, genericEnumParseMethodSupported);
        var members = ctx.SymbolAccessor.GetAllFields(ctx.Target);
        if (fallbackMapping.FallbackMember != null)
        {
            // no need to explicitly map fallback value
            members = members.Where(x => fallbackMapping.FallbackMember.ConstantValue?.Equals(x.ConstantValue) != true);
        }

        EnumMappingDiagnosticReporter.AddUnmatchedTargetIgnoredMembers(ctx, ignoredTargetMembers);
        return new EnumFromStringSwitchMapping(
            ctx.Source,
            ctx.Target,
            members,
            ctx.Configuration.Enum.IgnoreCase,
            fallbackMapping,
            explicitMappings
        );
    }

    private static EnumFallbackValueMapping BuildFallbackParseMapping(MappingBuilderContext ctx, bool genericEnumParseMethodSupported)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue == null)
        {
            return new EnumFallbackValueMapping(
                ctx.Source,
                ctx.Target,
                new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, ctx.Configuration.Enum.IgnoreCase)
            );
        }

        if (SymbolEqualityComparer.Default.Equals(ctx.Target, fallbackValue.Type))
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackMember: fallbackValue);

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumFallbackValueTypeDoesNotMatchTargetEnumType,
            fallbackValue,
            fallbackValue.ConstantValue ?? 0,
            fallbackValue.Type,
            ctx.Target
        );
        return new EnumFallbackValueMapping(
            ctx.Source,
            ctx.Target,
            new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, ctx.Configuration.Enum.IgnoreCase)
        );
    }

    private static IReadOnlyDictionary<IFieldSymbol, HashSet<ExpressionSyntax>> BuildExplicitValueMappings(MappingBuilderContext ctx)
    {
        var explicitMappings = new Dictionary<IFieldSymbol, HashSet<ExpressionSyntax>>(SymbolTypeEqualityComparer.FieldDefault);
        var checkedSources = new HashSet<object?>();
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            var targetField = ctx.SymbolAccessor.GetEnumField(target.ConstantValue)!;

            if (!SymbolEqualityComparer.Default.Equals(target.ConstantValue.Type, ctx.Target))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                    targetField,
                    target.ConstantValue.Value ?? 0,
                    target.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                    ctx.Target
                );
                continue;
            }

            if (!checkedSources.Add(source.ConstantValue.Value))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.StringSourceValueDuplicated,
                    source.Expression.ToFullString(),
                    ctx.Source,
                    ctx.Target
                );
            }

            if (explicitMappings.TryGetValue(targetField, out var sources))
            {
                sources.Add(source.Expression);
            }
            else
            {
                explicitMappings.Add(targetField, [source.Expression]);
            }
        }

        return explicitMappings;
    }
}
