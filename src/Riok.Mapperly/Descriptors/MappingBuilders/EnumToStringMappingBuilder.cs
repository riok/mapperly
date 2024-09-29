using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

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
        var fallbackMapping = BuildFallbackMapping(ctx, out var fallbackStringValue);
        var enumMemberMappings = BuildEnumMemberMappings(ctx, fallbackStringValue);
        return new EnumToStringMapping(ctx.Source, ctx.Target, enumMemberMappings, fallbackMapping);
    }

    private static IEnumerable<EnumMemberMapping> BuildEnumMemberMappings(MappingBuilderContext ctx, string? fallbackStringValue)
    {
        var ignoredSourceMembers = ctx.Configuration.Enum.IgnoredSourceMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        EnumMappingDiagnosticReporter.AddUnmatchedSourceIgnoredMembers(ctx, ignoredSourceMembers);

        var sourceFields = ctx.SymbolAccessor.GetFieldsExcept(ctx.Source, ignoredSourceMembers);
        var explicitValueMappings = BuildExplicitValueMappings(ctx);

        foreach (var sourceField in sourceFields)
        {
            // source.Value1
            var sourceSyntax = MemberAccess(FullyQualifiedIdentifier(ctx.Source), sourceField.Name);
            if (explicitValueMappings.TryGetValue(sourceField, out var memberName))
            {
                if (string.Equals(fallbackStringValue, memberName, StringComparison.Ordinal))
                    continue;

                // "explicit_value1"
                yield return new EnumMemberMapping(sourceSyntax, StringLiteral(memberName));
                continue;
            }

            var name = EnumMappingBuilder.GetMemberName(ctx, sourceField);
            if (string.Equals(name, fallbackStringValue, StringComparison.Ordinal))
                continue;

            if (ctx.Configuration.Enum.NamingStrategy == EnumNamingStrategy.MemberName)
            {
                // nameof(source.Value1)
                yield return new EnumMemberMapping(sourceSyntax, NameOf(sourceSyntax));
                continue;
            }

            // "value1"
            yield return new EnumMemberMapping(sourceSyntax, StringLiteral(name));
        }
    }

    private static IReadOnlyDictionary<IFieldSymbol, string> BuildExplicitValueMappings(MappingBuilderContext ctx)
    {
        var explicitMappings = new Dictionary<IFieldSymbol, string>(SymbolEqualityComparer.Default);
        if (!ctx.Configuration.Enum.HasExplicitConfigurations)
            return explicitMappings;

        var sourceFields = ctx.SymbolAccessor.GetEnumFieldsByValue(ctx.Source);
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (
                !SymbolEqualityComparer.Default.Equals(source.ConstantValue.Type, ctx.Source)
                || !sourceFields.TryGetValue(source.ConstantValue.Value!, out var sourceField)
            )
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

            if (target.ConstantValue.Value is not string targetStringValue)
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumExplicitMappingTargetNotString);
                continue;
            }

            if (!explicitMappings.TryAdd(sourceField, targetStringValue))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumSourceValueDuplicated, sourceField, ctx.Source, ctx.Target);
            }
        }

        return explicitMappings;
    }

    private static EnumFallbackValueMapping BuildFallbackMapping(MappingBuilderContext ctx, out string? fallbackStringValue)
    {
        fallbackStringValue = null;
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue is null)
        {
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, new ToStringMapping(ctx.Source, ctx.Target));
        }

        if (fallbackValue.Value.ConstantValue.Value is not string fallbackString)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidEnumMappingFallbackValue, fallbackValue.Value.Expression.ToFullString());
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, new ToStringMapping(ctx.Source, ctx.Target));
        }

        fallbackStringValue = fallbackString;
        return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackExpression: StringLiteral(fallbackString));
    }
}
