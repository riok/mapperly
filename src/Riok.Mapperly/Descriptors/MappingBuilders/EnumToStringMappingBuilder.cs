using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class EnumToStringMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.EnumToString))
            return null;

        if (ctx.Target.SpecialType != SpecialType.System_String || !ctx.Source.IsEnum())
            return null;

        // to string => use an optimized method of Enum.ToString which would use slow reflection
        // use Enum.ToString as fallback (for ex. for flags)
        return BuildEnumToStringMapping(ctx);
    }

    private static EnumToStringMapping BuildEnumToStringMapping(MappingBuilderContext ctx)
    {
        var ignoredSourceMembers = ctx.Configuration.Enum.IgnoredSourceMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        var explicitMappings = BuildExplicitValueMappings(ctx);

        EnumMappingDiagnosticReporter.AddUnmatchedSourceIgnoredMembers(ctx, ignoredSourceMembers);
        return new EnumToStringMapping(ctx.Source, ctx.Target, ctx.SymbolAccessor.GetAllFields(ctx.Source), explicitMappings);
    }

    private static IReadOnlyDictionary<IFieldSymbol, ExpressionSyntax> BuildExplicitValueMappings(MappingBuilderContext ctx)
    {
        var explicitMappings = new Dictionary<IFieldSymbol, ExpressionSyntax>(SymbolEqualityComparer.Default);
        var sourceFields = ctx.SymbolAccessor.GetEnumFields(ctx.Source);
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (!SymbolEqualityComparer.Default.Equals(source.ConstantValue.Type, ctx.Source))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                    source.Expression.ToFullString(),
                    source.ConstantValue.Value ?? 0,
                    source.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                    ctx.Source
                );
                continue;
            }

            if (!sourceFields.TryGetValue(source.ConstantValue.Value!, out var sourceField))
            {
                continue;
            }

            if (!explicitMappings.TryAdd(sourceField, target.Expression))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumSourceValueDuplicated, sourceField, ctx.Source, ctx.Target);
            }
        }

        return explicitMappings;
    }
}
