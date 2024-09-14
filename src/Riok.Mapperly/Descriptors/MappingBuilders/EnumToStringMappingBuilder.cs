using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
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
        var enumMemberMappings = BuildEnumMemberMappings(ctx);
        var fallbackMapping = BuildFallbackMapping(ctx);

        if (fallbackMapping.FallbackExpression is LiteralExpressionSyntax fallbackLiteral)
        {
            enumMemberMappings = enumMemberMappings.Where(IsNotEquivalentTo(fallbackLiteral));
        }

        return new EnumToStringMapping(ctx.Source, ctx.Target, enumMemberMappings, fallbackMapping);
    }

    private static IEnumerable<EnumMemberMapping> BuildEnumMemberMappings(MappingBuilderContext ctx)
    {
        var namingStrategy = ctx.Configuration.Enum.NamingStrategy;

        var ignoredSourceMembers = ctx.Configuration.Enum.IgnoredSourceMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        EnumMappingDiagnosticReporter.AddUnmatchedSourceIgnoredMembers(ctx, ignoredSourceMembers);

        var fields = ctx.SymbolAccessor.GetFieldsExcept(ctx.Source, ignoredSourceMembers);
        var explicitValueMappings = BuildExplicitValueMappings(ctx);
        var customNameMappings = ctx.BuildCustomNameStrategyMappings(ctx.Source);

        foreach (var field in fields)
        {
            // source.Value1
            var sourceSyntax = MemberAccess(FullyQualifiedIdentifier(ctx.Source), field.Name);

            ExpressionSyntax targetSyntax;
            if (explicitValueMappings.TryGetValue(field, out var explicitMapping))
            {
                // "explicit_value1"
                targetSyntax = explicitMapping;
            }
            else if (
                namingStrategy is not EnumNamingStrategy.MemberName
                && customNameMappings.TryGetValue(field, out var customNameMapping)
            )
            {
                // "value1"
                targetSyntax = StringLiteral(customNameMapping);
            }
            else
            {
                // nameof(source.Value1)
                targetSyntax = NameOf(sourceSyntax);
            }

            // source.Value1 => nameof(source.Value1)
            yield return new EnumMemberMapping(sourceSyntax, targetSyntax);
        }
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

    private static EnumFallbackValueMapping BuildFallbackMapping(MappingBuilderContext ctx)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue is null)
        {
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, new EnumFallbackToStringMapping(ctx.Source, ctx.Target));
        }

        if (fallbackValue is not { Expression: LiteralExpressionSyntax literalExpressionSyntax })
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidFallbackValue, fallbackValue.Value.Expression.ToFullString());
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, new EnumFallbackToStringMapping(ctx.Source, ctx.Target));
        }

        if (SymbolEqualityComparer.Default.Equals(ctx.Target, fallbackValue.Value.ConstantValue.Type))
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackExpression: literalExpressionSyntax);

        return new EnumFallbackValueMapping(ctx.Source, ctx.Target, new EnumFallbackToStringMapping(ctx.Source, ctx.Target));
    }

    private static Func<EnumMemberMapping, bool> IsNotEquivalentTo(LiteralExpressionSyntax fallbackLiteral) =>
        mapping =>
            !(
                mapping.TargetSyntax is LiteralExpressionSyntax targetLiteral
                && fallbackLiteral.ToString().Equals(targetLiteral.ToString(), StringComparison.Ordinal)
            );
}
