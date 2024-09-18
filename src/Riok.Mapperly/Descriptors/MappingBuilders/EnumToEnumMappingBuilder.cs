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

public static class EnumToEnumMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var sourceIsEnum = ctx.Source.TryGetEnumUnderlyingType(out var sourceEnumType);
        var targetIsEnum = ctx.Target.TryGetEnumUnderlyingType(out var targetEnumType);

        // none is an enum
        if (!sourceIsEnum && !targetIsEnum)
            return null;

        // one is an enum, other may be an underlying type (eg. int)
        if (!sourceIsEnum || !targetIsEnum)
        {
            return
                ctx.IsConversionEnabled(MappingConversionType.EnumUnderlyingType)
                && ctx.FindOrBuildMapping(sourceEnumType ?? ctx.Source, targetEnumType ?? ctx.Target) is { } delegateMapping
                ? new CastMapping(ctx.Source, ctx.Target, delegateMapping)
                : null;
        }

        if (!ctx.IsConversionEnabled(MappingConversionType.EnumToEnum))
            return null;

        // map enums by strategy
        return ctx.Configuration.Enum.Strategy switch
        {
            EnumMappingStrategy.ByName when ctx.IsExpression => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByValue when ctx is { IsExpression: true, Configuration.Enum.HasExplicitConfigurations: true } =>
                BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByValueCheckDefined when ctx.IsExpression => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByName => BuildNameMapping(ctx),
            EnumMappingStrategy.ByValueCheckDefined => BuildEnumToEnumCastMapping(ctx, checkTargetDefined: true),
            _ => BuildEnumToEnumCastMapping(ctx),
        };
    }

    private static INewInstanceMapping BuildCastMappingAndDiagnostic(MappingBuilderContext ctx)
    {
        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumMappingNotSupportedInProjectionMappings,
            ctx.Source.ToDisplayString(),
            ctx.Target.ToDisplayString()
        );
        return BuildEnumToEnumCastMapping(ctx, true);
    }

    private static INewInstanceMapping BuildEnumToEnumCastMapping(
        MappingBuilderContext ctx,
        bool ignoreExplicitAndIgnoredMappings = false,
        bool checkTargetDefined = false
    )
    {
        var enumMemberMappings = BuildEnumMemberMappings(
            ctx,
            ignoreExplicitAndIgnoredMappings,
            static x => x.ConstantValue!,
            EqualityComparer<object>.Default
        );
        var fallbackMapping = BuildFallbackMapping(ctx);
        if (fallbackMapping.FallbackExpression is not null && !checkTargetDefined)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.EnumFallbackValueRequiresByValueCheckDefinedStrategy);
            checkTargetDefined = true;
        }

        var checkDefinedMode = checkTargetDefined switch
        {
            false => EnumCastMapping.CheckDefinedMode.NoCheck,
            _ when ctx.SymbolAccessor.HasAttribute<FlagsAttribute>(ctx.Target) => EnumCastMapping.CheckDefinedMode.Flags,
            _ => EnumCastMapping.CheckDefinedMode.Value,
        };

        var castFallbackMapping = new EnumCastMapping(
            ctx.Source,
            ctx.Target,
            checkDefinedMode,
            enumMemberMappings.TargetMembers,
            fallbackMapping
        );
        var differentValueExplicitEnumMappings = enumMemberMappings
            .ExplicitMemberMappings.Where(x => x.Key.ConstantValue?.Equals(x.Value.ConstantValue) != true)
            .ToDictionary(x => x.Key, x => x.Value, SymbolTypeEqualityComparer.FieldDefault);

        if (differentValueExplicitEnumMappings.Count == 0)
            return castFallbackMapping;

        return new EnumNameMapping(
            ctx.Source,
            ctx.Target,
            differentValueExplicitEnumMappings,
            new EnumFallbackValueMapping(ctx.Source, ctx.Target, castFallbackMapping)
        );
    }

    private static EnumNameMapping BuildNameMapping(MappingBuilderContext ctx)
    {
        var fallbackMapping = BuildFallbackMapping(ctx);
        var enumMemberMappings = ctx.Configuration.Enum.IgnoreCase
            ? BuildEnumMemberMappings(ctx, false, static x => x.Name, StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase)
            : BuildEnumMemberMappings(ctx, false, static x => x.Name, StringComparer.Ordinal);

        if (enumMemberMappings.MemberMappings.Count == 0)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.EnumNameMappingNoOverlappingValuesFound, ctx.Source, ctx.Target);
        }

        return new EnumNameMapping(ctx.Source, ctx.Target, enumMemberMappings.MemberMappings, fallbackMapping);
    }

    private static EnumMemberMappings BuildEnumMemberMappings<T>(
        MappingBuilderContext ctx,
        bool ignoreExplicitAndIgnoredMappings,
        Func<IFieldSymbol, T> propertySelector,
        params IEqualityComparer<T>[] propertyComparer
    )
        where T : notnull
    {
        var ignoredSourceMembers = ignoreExplicitAndIgnoredMappings
            ? new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default)
            : ctx.Configuration.Enum.IgnoredSourceMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        var ignoredTargetMembers = ignoreExplicitAndIgnoredMappings
            ? new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default)
            : ctx.Configuration.Enum.IgnoredTargetMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        var explicitMappings = ignoreExplicitAndIgnoredMappings
            ? new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default)
            : BuildExplicitValueMappings(ctx);
        var sourceMembers = ctx.SymbolAccessor.GetFieldsExcept(ctx.Source, ignoredSourceMembers);
        var targetMembers = ctx.SymbolAccessor.GetFieldsExcept(ctx.Target, ignoredTargetMembers);

        var targetMembersByProperty = propertyComparer
            .Select(pc => targetMembers.DistinctBy(propertySelector, pc).ToDictionary(propertySelector, x => x, pc))
            .ToList();

        var mappedTargetMembers = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var mappings = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);
        foreach (var sourceMember in sourceMembers)
        {
            if (!explicitMappings.TryGetValue(sourceMember, out var targetMember))
            {
                var sourceProperty = propertySelector(sourceMember);
                foreach (var targetMembersByPropertyCandidate in targetMembersByProperty)
                {
                    if (targetMembersByPropertyCandidate.TryGetValue(sourceProperty, out targetMember))
                        break;
                }

                if (targetMember == null)
                    continue;
            }

            mappings.Add(sourceMember, targetMember);
            mappedTargetMembers.Add(targetMember);
        }

        EnumMappingDiagnosticReporter.AddUnmappedSourceMembersDiagnostics(ctx, mappings.Keys.ToHashSet(), sourceMembers);
        EnumMappingDiagnosticReporter.AddUnmappedTargetMembersDiagnostics(ctx, mappedTargetMembers, targetMembers);
        EnumMappingDiagnosticReporter.AddUnmatchedSourceIgnoredMembers(ctx, ignoredSourceMembers);
        EnumMappingDiagnosticReporter.AddUnmatchedTargetIgnoredMembers(ctx, ignoredTargetMembers);
        return new EnumMemberMappings(mappings, explicitMappings, targetMembers);
    }

    private static EnumFallbackValueMapping BuildFallbackMapping(MappingBuilderContext ctx)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue is null)
        {
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target);
        }

        if (fallbackValue is not { Expression: MemberAccessExpressionSyntax memberAccessExpression })
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidEnumMappingFallbackValue, fallbackValue.Value.Expression.ToFullString());
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target);
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
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target);
        }

        var fallbackExpression = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            FullyQualifiedIdentifier(ctx.Target),
            memberAccessExpression.Name
        );
        return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackExpression: fallbackExpression);
    }

    private static IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> BuildExplicitValueMappings(MappingBuilderContext ctx)
    {
        var explicitMappings = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);
        var sourceFields = ctx.SymbolAccessor.GetEnumFieldsByValue(ctx.Source);
        var targetFields = ctx.SymbolAccessor.GetEnumFieldsByValue(ctx.Target);
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (source.ConstantValue.Kind is not TypedConstantKind.Enum)
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.MapValueTypeMismatch,
                    source.Expression.ToFullString(),
                    source.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                    ctx.Source
                );
                continue;
            }

            if (target.ConstantValue.Kind is not TypedConstantKind.Enum)
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.MapValueTypeMismatch,
                    target.Expression.ToFullString(),
                    target.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                    ctx.Target
                );
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(source.ConstantValue.Type, ctx.Source))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                    target.Expression.ToFullString(),
                    target.ConstantValue.Value ?? 0,
                    target.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                    ctx.Source
                );
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(target.ConstantValue.Type, ctx.Target))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                    source.Expression.ToFullString(),
                    source.ConstantValue.Value ?? 0,
                    source.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                    ctx.Target
                );
                continue;
            }

            if (
                !sourceFields.TryGetValue(source.ConstantValue.Value!, out var sourceField)
                || !targetFields.TryGetValue(target.ConstantValue.Value!, out var targetField)
            )
            {
                continue;
            }

            if (!explicitMappings.TryAdd(sourceField, targetField))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumSourceValueDuplicated, sourceField, ctx.Source, ctx.Target);
            }
        }

        return explicitMappings;
    }

    private record EnumMemberMappings(
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> MemberMappings,
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> ExplicitMemberMappings,
        IReadOnlyCollection<IFieldSymbol> TargetMembers
    );
}
