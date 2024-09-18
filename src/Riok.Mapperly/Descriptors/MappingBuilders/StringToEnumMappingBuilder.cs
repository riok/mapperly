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
        // from string => use an optimized method of Enum.Parse which would use slow reflection
        // however we currently don't support all features of Enum.Parse yet (ex. flags)
        // therefore we use Enum.Parse as fallback.
        var fallbackMapping = BuildFallbackParseMapping(ctx, genericEnumParseMethodSupported, out var fallbackMember);
        var enumMemberMappings = BuildEnumMemberMappings(ctx, fallbackMember);
        return new EnumFromStringSwitchMapping(
            ctx.Source,
            ctx.Target,
            ctx.Configuration.Enum.IgnoreCase,
            enumMemberMappings,
            fallbackMapping
        );
    }

    private static IEnumerable<EnumMemberMapping> BuildEnumMemberMappings(MappingBuilderContext ctx, IFieldSymbol? fallbackMember)
    {
        var ignoredTargetMembers = ctx.Configuration.Enum.IgnoredTargetMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        EnumMappingDiagnosticReporter.AddUnmatchedTargetIgnoredMembers(ctx, ignoredTargetMembers);
        if (fallbackMember is not null)
        {
            ignoredTargetMembers.Add(fallbackMember);
        }

        var targetFields = ctx.SymbolAccessor.GetFieldsExcept(ctx.Target, ignoredTargetMembers);
        var explicitValueMappings = BuildExplicitValueMappings(ctx);
        var processedSources = new HashSet<object?>();

        foreach (var targetField in targetFields)
        {
            // source.Value1
            var targetSyntax = MemberAccess(FullyQualifiedIdentifier(ctx.Target), targetField.Name);

            if (explicitValueMappings.TryGetValue(targetField, out var sourceNames))
            {
                foreach (var sourceName in sourceNames)
                {
                    if (!processedSources.Add(sourceName))
                    {
                        ctx.ReportDiagnostic(DiagnosticDescriptors.EnumStringSourceValueDuplicated, sourceName, ctx.Source, ctx.Target);
                        continue;
                    }

                    // "explicit_value1" => source.Value1
                    yield return new EnumMemberMapping(StringLiteral(sourceName), targetSyntax);
                }
                continue;
            }

            var name = EnumMappingBuilder.GetMemberName(ctx, targetField);
            if (!processedSources.Add(name))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumStringSourceValueDuplicated, name, ctx.Source, ctx.Target);
                continue;
            }

            if (ctx.Configuration.Enum.NamingStrategy == EnumNamingStrategy.MemberName)
            {
                // nameof(source.Value1)
                yield return new EnumMemberMapping(NameOf(targetSyntax), targetSyntax);
                continue;
            }

            // "value1"
            yield return new EnumMemberMapping(StringLiteral(name), targetSyntax);
        }
    }

    private static EnumFallbackValueMapping BuildFallbackParseMapping(
        MappingBuilderContext ctx,
        bool genericEnumParseMethodSupported,
        out IFieldSymbol? fallbackMember
    )
    {
        fallbackMember = null;
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
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidEnumMappingFallbackValue, fallbackValue.Value.Expression.ToFullString());
            return new EnumFallbackValueMapping(
                ctx.Source,
                ctx.Target,
                new EnumFromStringParseMapping(ctx.Source, ctx.Target, genericEnumParseMethodSupported, ctx.Configuration.Enum.IgnoreCase)
            );
        }

        if (!SymbolEqualityComparer.Default.Equals(ctx.Target, fallbackValue.Value.ConstantValue.Type))
        {
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

        var fallbackExpression = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            FullyQualifiedIdentifier(ctx.Target),
            memberAccessExpression.Name
        );
        fallbackMember = ctx.SymbolAccessor.GetField(ctx.Target, memberAccessExpression.Name.Identifier.Text);
        return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackExpression: fallbackExpression);
    }

    private static IReadOnlyDictionary<IFieldSymbol, HashSet<string>> BuildExplicitValueMappings(MappingBuilderContext ctx)
    {
        var explicitMappings = new Dictionary<IFieldSymbol, HashSet<string>>(SymbolTypeEqualityComparer.FieldDefault);
        var targetFields = ctx.SymbolAccessor.GetEnumFieldsByValue(ctx.Target);
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (
                !SymbolEqualityComparer.Default.Equals(target.ConstantValue.Type, ctx.Target)
                || !targetFields.TryGetValue(target.ConstantValue.Value!, out var targetField)
            )
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

            if (source.ConstantValue.Value is not string sourceString)
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumExplicitMappingSourceNotString);
                continue;
            }

            if (explicitMappings.TryGetValue(targetField, out var sources))
            {
                sources.Add(sourceString);
            }
            else
            {
                explicitMappings.Add(targetField, [sourceString]);
            }
        }

        return explicitMappings;
    }
}
