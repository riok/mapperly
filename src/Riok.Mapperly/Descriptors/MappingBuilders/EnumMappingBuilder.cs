using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class EnumMappingBuilder
{
    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
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
                ctx.IsConversionEnabled(MappingConversionType.ExplicitCast)
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
            EnumMappingStrategy.ByValue when ctx is { IsExpression: true, Configuration.Enum.HasExplicitConfigurations: true }
                => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByValueCheckDefined when ctx.IsExpression => BuildCastMappingAndDiagnostic(ctx),
            EnumMappingStrategy.ByName => BuildNameMapping(ctx),
            EnumMappingStrategy.ByValueCheckDefined => BuildEnumToEnumCastMapping(ctx, checkTargetDefined: true),
            _ => BuildEnumToEnumCastMapping(ctx),
        };
    }

    private static TypeMapping BuildCastMappingAndDiagnostic(MappingBuilderContext ctx)
    {
        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumMappingNotSupportedInProjectionMappings,
            ctx.Source.ToDisplayString(),
            ctx.Target.ToDisplayString()
        );
        return BuildEnumToEnumCastMapping(ctx, true);
    }

    private static TypeMapping BuildEnumToEnumCastMapping(
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
        if (fallbackMapping.FallbackMember != null && !checkTargetDefined)
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
        var differentValueExplicitEnumMappings = enumMemberMappings.ExplicitMemberMappings
            .Where(x => x.Key.ConstantValue?.Equals(x.Value.ConstantValue) != true)
            .ToDictionary(x => x.Key, x => x.Value, (IEqualityComparer<IFieldSymbol>)SymbolEqualityComparer.Default);

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
    {
        var ignoredSourceMembers = ignoreExplicitAndIgnoredMappings
            ? new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default)
            : ctx.Configuration.Enum.IgnoredSourceMembers.ToHashSet();
        var ignoredTargetMembers = ignoreExplicitAndIgnoredMappings
            ? new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default)
            : ctx.Configuration.Enum.IgnoredTargetMembers.ToHashSet();
        var explicitMappings = ignoreExplicitAndIgnoredMappings
            ? new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default)
            : BuildExplicitValueMappings(ctx);
        var sourceMembers = ctx.Source.GetMembers().OfType<IFieldSymbol>().Where(x => !ignoredSourceMembers.Remove(x)).ToHashSet();
        var targetMembers = ctx.Target.GetMembers().OfType<IFieldSymbol>().Where(x => !ignoredTargetMembers.Remove(x)).ToHashSet();

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

        AddUnmappedMembersDiagnostics(ctx, mappings, mappedTargetMembers, sourceMembers, targetMembers);
        AddUnmatchedIgnoredMembers(ctx, ignoredSourceMembers, ignoredTargetMembers);
        return new EnumMemberMappings(mappings, explicitMappings, targetMembers);
    }

    private static void AddUnmatchedIgnoredMembers(
        MappingBuilderContext ctx,
        ISet<IFieldSymbol> ignoredUnmatchedSourceMembers,
        ISet<IFieldSymbol> ignoredUnmatchedTargetMembers
    )
    {
        foreach (var member in ignoredUnmatchedSourceMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredEnumSourceMemberNotFound,
                member.Name,
                member.ConstantValue!,
                ctx.Source,
                ctx.Target
            );
        }

        foreach (var member in ignoredUnmatchedTargetMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.IgnoredEnumTargetMemberNotFound,
                member.Name,
                member.ConstantValue!,
                ctx.Source,
                ctx.Target
            );
        }
    }

    private static void AddUnmappedMembersDiagnostics(
        MappingBuilderContext ctx,
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> mappings,
        ISet<IFieldSymbol> mappedTargetMembers,
        IEnumerable<IFieldSymbol> sourceMembers,
        IEnumerable<IFieldSymbol> targetMembers
    )
    {
        var missingSourceMembers = sourceMembers.Where(field => !mappings.ContainsKey(field));
        foreach (var member in missingSourceMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.SourceEnumValueNotMapped,
                member.Name,
                member.ConstantValue!,
                ctx.Source,
                ctx.Target
            );
        }

        var missingTargetMembers = targetMembers.Where(
            field =>
                !mappedTargetMembers.Contains(field)
                && ctx.Configuration.Enum.FallbackValue?.ConstantValue?.Equals(field.ConstantValue) != true
        );
        foreach (var member in missingTargetMembers)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.TargetEnumValueNotMapped,
                member.Name,
                member.ConstantValue!,
                ctx.Target,
                ctx.Source
            );
        }
    }

    private static EnumFallbackValueMapping BuildFallbackMapping(MappingBuilderContext ctx)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue == null)
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target);

        if (SymbolEqualityComparer.Default.Equals(ctx.Target, fallbackValue.Type))
            return new EnumFallbackValueMapping(ctx.Source, ctx.Target, fallbackMember: fallbackValue);

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumFallbackValueTypeDoesNotMatchTargetEnumType,
            fallbackValue,
            fallbackValue.ConstantValue ?? 0,
            fallbackValue.Type,
            ctx.Target
        );
        return new EnumFallbackValueMapping(ctx.Source, ctx.Target);
    }

    private static IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> BuildExplicitValueMappings(MappingBuilderContext ctx)
    {
        var targetFieldsByExplicitValue = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);
        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (!SymbolEqualityComparer.Default.Equals(source.Type, ctx.Source))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                    source,
                    source.ConstantValue ?? 0,
                    source.Type,
                    ctx.Source
                );
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(target.Type, ctx.Target))
            {
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
                    target,
                    target.ConstantValue ?? 0,
                    target.Type,
                    ctx.Target
                );
                continue;
            }

            if (targetFieldsByExplicitValue.ContainsKey(source))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumSourceValueDuplicated, source, ctx.Source, ctx.Target);
            }
            else
            {
                targetFieldsByExplicitValue.Add(source, target);
            }
        }

        return targetFieldsByExplicitValue;
    }

    private record EnumMemberMappings(
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> MemberMappings,
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> ExplicitMemberMappings,
        IReadOnlyCollection<IFieldSymbol> TargetMembers
    );
}
