using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        var enumMemberMappings = BuildEnumMemberMappings(ctx);
        // from string => use an optimized method of Enum.Parse which would use slow reflection
        // however we currently don't support all features of Enum.Parse yet (ex. flags)
        // therefore we use Enum.Parse as fallback.
        var fallbackMapping = BuildFallbackParseMapping(ctx, genericEnumParseMethodSupported);
        if (fallbackMapping.FallbackExpression is MemberAccessExpressionSyntax fallbackMember)
        {
            // no need to explicitly map fallback value
            enumMemberMappings = enumMemberMappings.Where(IsNotEquivalentTo(fallbackMember));
        }

        return new EnumFromStringSwitchMapping(
            ctx.Source,
            ctx.Target,
            ctx.Configuration.Enum.IgnoreCase,
            enumMemberMappings,
            fallbackMapping
        );
    }

    private static IEnumerable<EnumMemberMapping> BuildEnumMemberMappings(MappingBuilderContext ctx)
    {
        var namingStrategy = ctx.Configuration.Enum.NamingStrategy;

        var ignoredTargetMembers = ctx.Configuration.Enum.IgnoredTargetMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        EnumMappingDiagnosticReporter.AddUnmatchedTargetIgnoredMembers(ctx, ignoredTargetMembers);

        var fields = ctx.SymbolAccessor.GetFieldsExcept(ctx.Target, ignoredTargetMembers);
        var explicitValueMappings = BuildExplicitValueMappings(ctx);
        var customNameMappings = ctx.BuildCustomNameStrategyMappings(ctx.Target);

        foreach (var field in fields)
        {
            // source.Value1
            var targetSyntax = MemberAccess(FullyQualifiedIdentifier(ctx.Target), field.Name);

            if (explicitValueMappings.TryGetValue(field, out var explicitMappings))
            {
                foreach (var explicitMapping in explicitMappings)
                {
                    // "explicit_value1" => source.Value1
                    yield return new EnumMemberMapping(explicitMapping, targetSyntax);
                }
                continue;
            }

            ExpressionSyntax sourceSyntax;
            if (namingStrategy is not EnumNamingStrategy.MemberName && customNameMappings.TryGetValue(field, out var customNameMapping))
            {
                // "value1"
                sourceSyntax = StringLiteral(customNameMapping);
            }
            else
            {
                // nameof(source.Value1)
                sourceSyntax = NameOf(targetSyntax);
            }

            // nameof(source.Value1) => source.Value1
            yield return new EnumMemberMapping(sourceSyntax, targetSyntax);
        }
    }

    private static EnumFallbackValueMapping BuildFallbackParseMapping(MappingBuilderContext ctx, bool genericEnumParseMethodSupported)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue is null)
        {
            return new EnumFallbackValueMapping(
                ctx.Source,
                ctx.Target,
                new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, ctx.Configuration.Enum.IgnoreCase)
            );
        }

        if (fallbackValue is not { Expression: MemberAccessExpressionSyntax memberAccessExpression })
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidFallbackValue, fallbackValue.Value.Expression.ToFullString());
            return new EnumFallbackValueMapping(
                ctx.Source,
                ctx.Target,
                new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, ctx.Configuration.Enum.IgnoreCase)
            );
        }

        var fallbackExpression = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            FullyQualifiedIdentifier(ctx.Target),
            memberAccessExpression.Name
        );

        if (SymbolEqualityComparer.Default.Equals(ctx.Target, fallbackValue.Value.ConstantValue.Type))
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackExpression: fallbackExpression);

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumFallbackValueTypeDoesNotMatchTargetEnumType,
            fallbackValue,
            fallbackValue.Value.ConstantValue.Value ?? 0,
            fallbackValue.Value.ConstantValue.Type?.Name ?? "unknown",
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
        var targetFields = ctx.SymbolAccessor.GetEnumFields(ctx.Target);
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (!SymbolEqualityComparer.Default.Equals(target.ConstantValue.Type, ctx.Target))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                    target.Expression.ToFullString(),
                    target.ConstantValue.Value ?? 0,
                    target.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                    ctx.Target
                );
                continue;
            }

            if (!targetFields.TryGetValue(target.ConstantValue.Value!, out var targetField))
            {
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
                continue;
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

    private static Func<EnumMemberMapping, bool> IsNotEquivalentTo(MemberAccessExpressionSyntax fallbackMember) =>
        mapping =>
            !(
                mapping.TargetSyntax is MemberAccessExpressionSyntax targetMember
                && fallbackMember.Expression.IsEquivalentTo(targetMember.Expression)
                && fallbackMember.Name.ToString().Equals(targetMember.Name.ToString(), StringComparison.Ordinal)
            );
}
