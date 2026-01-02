using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.Enumerables;
using Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.Enums;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class EnumToEnumMappingBuilder
{
    public static INewInstanceMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        var (isEnumerableTarget, actualTargetEnumType) = DetermineEnumerableTarget(ctx);

        var sourceIsEnum = ctx.Source.TryGetEnumUnderlyingType(out var sourceEnumType);
        var targetIsEnum = actualTargetEnumType.IsEnum();

        // none is an enum
        if (!sourceIsEnum && !targetIsEnum)
            return null;

        // one is an enum, other may be an underlying type (e.g. int)
        if (!sourceIsEnum || !targetIsEnum)
        {
            return TryBuildUnderlyingTypeMapping(ctx, sourceEnumType, actualTargetEnumType);
        }

        if (!ctx.IsConversionEnabled(MappingConversionType.EnumToEnum))
            return null;

        // Check for multiple enum parameters
        return TryGetMultipleEnumParameters(ctx, out var enumParams)
            ? BuildMultiSourceEnumMapping(ctx, enumParams, actualTargetEnumType, isEnumerableTarget)
            : BuildSingleSourceMapping(ctx);
    }

    private static (bool isEnumerableTarget, ITypeSymbol actualTargetEnumType) DetermineEnumerableTarget(MappingBuilderContext ctx)
    {
        var isEnumerableTarget = false;
        var actualTargetEnumType = ctx.Target;

        var enumeratedType = CollectionInfoBuilder.GetEnumeratedType(ctx.Types, ctx.Target);
        if (enumeratedType is null)
        {
            return (isEnumerableTarget, actualTargetEnumType);
        }

        var targetInfo = CollectionInfoBuilder.BuildCollectionInfo(ctx.Types, ctx.SymbolAccessor, ctx.Target, enumeratedType);

        if (!targetInfo.ImplementsIEnumerable)
        {
            return (isEnumerableTarget, actualTargetEnumType);
        }

        isEnumerableTarget = true;
        actualTargetEnumType = enumeratedType;

        return (isEnumerableTarget, actualTargetEnumType);
    }

    private static INewInstanceMapping? TryBuildUnderlyingTypeMapping(
        MappingBuilderContext ctx,
        ITypeSymbol? sourceEnumType,
        ITypeSymbol targetType
    )
    {
        // If enum underlying type conversion is disabled, don't block other conversions
        if (!ctx.IsConversionEnabled(MappingConversionType.EnumUnderlyingType))
            return null;

        // Get the underlying type if target is an enum
        targetType.TryGetEnumUnderlyingType(out var targetUnderlyingType);

        var delegateMapping = ctx.FindOrBuildMapping(sourceEnumType ?? ctx.Source, targetUnderlyingType ?? ctx.Target);

        return delegateMapping is not null ? new CastMapping(ctx.Source, ctx.Target, delegateMapping) : null;
    }

    private static bool TryGetMultipleEnumParameters(MappingBuilderContext ctx, out IReadOnlyList<MethodParameter> enumParams)
    {
        enumParams = [];

        if (ctx.UserMapping?.Method.Parameters is not { Length: > 1 } parameters)
            return false;

        var paramList = new List<MethodParameter>(parameters.Length);
        foreach (var param in parameters)
        {
            if (param.Type.TryGetEnumUnderlyingType(out _))
            {
                paramList.Add(ctx.SymbolAccessor.WrapMethodParameter(param));
            }
        }

        if (paramList.Count <= 1)
            return false;

        enumParams = paramList;
        return true;
    }

    private static INewInstanceMapping BuildSingleSourceMapping(MappingBuilderContext ctx)
    {
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

    #region Multi-Source Enum Mapping

    private static INewInstanceMapping BuildMultiSourceEnumMapping(
        MappingBuilderContext ctx,
        IReadOnlyList<MethodParameter> allEnumParameters,
        ITypeSymbol targetEnumType,
        bool useYieldReturn
    )
    {
        var mappedTargetMembers = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var sourceMappings = new List<EnumSourceMapping>(allEnumParameters.Count);

        foreach (var param in allEnumParameters)
        {
            var memberMappings = BuildEnumMemberMappingsForParameter(ctx, param, targetEnumType, mappedTargetMembers);
            sourceMappings.Add(new EnumSourceMapping(param, memberMappings));
        }

        var fallbackMapping = BuildFallbackMapping(ctx, targetEnumType);
        return new EnumMultiSourceMapping(allEnumParameters[0].Type, ctx.Target, sourceMappings, fallbackMapping, useYieldReturn);
    }

    private static IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> BuildEnumMemberMappingsForParameter(
        MappingBuilderContext ctx,
        MethodParameter parameter,
        ITypeSymbol targetEnumType,
        HashSet<IFieldSymbol> alreadyMappedTargets
    )
    {
        var ignoredSourceSet = ctx.Configuration.Enum.IgnoredSourceMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);
        var ignoredTargetSet = ctx.Configuration.Enum.IgnoredTargetMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault);

        var sourceMembers = ctx.SymbolAccessor.GetFieldsExcept(parameter.Type, ignoredSourceSet);
        var targetMembers = ctx.SymbolAccessor.GetFieldsExcept(targetEnumType, ignoredTargetSet);

        var explicitMappings = BuildExplicitValueMappingsForSource(ctx, parameter.Type, targetEnumType);
        var mappings = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);

        foreach (var sourceMember in sourceMembers)
        {
            var targetMember = FindTargetMemberForSource(
                ctx,
                sourceMember,
                targetMembers,
                explicitMappings,
                targetEnumType,
                alreadyMappedTargets
            );

            if (targetMember is not null && alreadyMappedTargets.Add(targetMember))
            {
                mappings.Add(sourceMember, targetMember);
            }
        }

        return mappings;
    }

    private static IFieldSymbol? FindTargetMemberForSource(
        MappingBuilderContext ctx,
        IFieldSymbol sourceMember,
        IReadOnlyCollection<IFieldSymbol> targetMembers,
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> explicitMappings,
        ITypeSymbol targetEnumType,
        HashSet<IFieldSymbol> alreadyMappedTargets
    )
    {
        // Check explicit mappings first
        if (explicitMappings.TryGetValue(sourceMember, out var targetMember))
            return targetMember;

        // Apply strategy-based matching
        return ctx.Configuration.Enum.Strategy switch
        {
            EnumMappingStrategy.ByValue or EnumMappingStrategy.ByValueCheckDefined => FindByValue(
                sourceMember,
                targetMembers,
                targetEnumType,
                alreadyMappedTargets
            ),

            EnumMappingStrategy.ByName => FindByName(ctx, sourceMember, targetMembers, alreadyMappedTargets),

            _ => FindByValue(sourceMember, targetMembers, targetEnumType, alreadyMappedTargets),
        };
    }

    private static IFieldSymbol? FindByValue(
        IFieldSymbol sourceMember,
        IReadOnlyCollection<IFieldSymbol> targetMembers,
        ITypeSymbol targetEnumType,
        HashSet<IFieldSymbol> alreadyMappedTargets
    )
    {
        var sourceValue = sourceMember.ConstantValue!;
        return targetMembers.FirstOrDefault(t =>
            !alreadyMappedTargets.Contains(t)
            && SymbolEqualityComparer.Default.Equals(t.ContainingType, targetEnumType)
            && Equals(t.ConstantValue, sourceValue)
        );
    }

    private static IFieldSymbol? FindByName(
        MappingBuilderContext ctx,
        IFieldSymbol sourceMember,
        IReadOnlyCollection<IFieldSymbol> targetMembers,
        HashSet<IFieldSymbol> alreadyMappedTargets
    )
    {
        var comparer = ctx.Configuration.Enum.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        var targetMembersByName = targetMembers
            .Where(t => !alreadyMappedTargets.Contains(t))
            .DistinctBy(t => t.Name, comparer)
            .ToDictionary(t => t.Name, t => t, comparer);

        return targetMembersByName.TryGetValue(sourceMember.Name, out var target) ? target : null;
    }

    private static IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> BuildExplicitValueMappingsForSource(
        MappingBuilderContext ctx,
        ITypeSymbol sourceType,
        ITypeSymbol targetEnumType
    )
    {
        var explicitMappings = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);
        var sourceFields = ctx.SymbolAccessor.GetEnumFieldsByValue(sourceType);
        var targetFields = ctx.SymbolAccessor.GetEnumFieldsByValue(targetEnumType);

        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            // Only process explicit mappings that match this source type
            if (!SymbolEqualityComparer.Default.Equals(source.ConstantValue.Type, sourceType))
                continue;

            if (!ValidateExplicitMappingTypes(ctx, source, target, sourceType, targetEnumType))
                continue;

            if (
                !TryGetMappingFields(
                    sourceFields,
                    targetFields,
                    source.ConstantValue,
                    target.ConstantValue,
                    out var sourceField,
                    out var targetField
                )
            )
                continue;

            if (!explicitMappings.TryAdd(sourceField, targetField))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumSourceValueDuplicated, sourceField, sourceType, targetEnumType);
            }
        }

        return explicitMappings;
    }

    #endregion

    #region Single-Source Enum Mapping

    private static INewInstanceMapping BuildCastMappingAndDiagnostic(MappingBuilderContext ctx)
    {
        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumMappingNotSupportedInProjectionMappings,
            ctx.Source.ToDisplayString(),
            ctx.Target.ToDisplayString()
        );

        return BuildEnumToEnumCastMapping(ctx, ignoreExplicitAndIgnoredMappings: true);
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

        var checkDefinedMode = DetermineCheckDefinedMode(ctx, checkTargetDefined);

        var castFallbackMapping = new EnumCastMapping(
            ctx.Source,
            ctx.Target,
            checkDefinedMode,
            enumMemberMappings.TargetMembers,
            fallbackMapping
        );

        var differentValueMappings = enumMemberMappings
            .ExplicitMemberMappings.Where(x => x.Key.ConstantValue?.Equals(x.Value.ConstantValue) != true)
            .ToDictionary(x => x.Key, x => x.Value, SymbolTypeEqualityComparer.FieldDefault);

        if (differentValueMappings.Count == 0)
            return castFallbackMapping;

        return new EnumNameMapping(
            ctx.Source,
            ctx.Target,
            differentValueMappings,
            new EnumFallbackValueMapping(ctx.Source, ctx.Target, castFallbackMapping)
        );
    }

    private static EnumCastMapping.CheckDefinedMode DetermineCheckDefinedMode(MappingBuilderContext ctx, bool checkTargetDefined)
    {
        return checkTargetDefined switch
        {
            false => EnumCastMapping.CheckDefinedMode.NoCheck,
            _ when ctx.SymbolAccessor.HasAttribute<FlagsAttribute>(ctx.Target) => EnumCastMapping.CheckDefinedMode.Flags,
            _ => EnumCastMapping.CheckDefinedMode.Value,
        };
    }

    private static EnumNameMapping BuildNameMapping(MappingBuilderContext ctx)
    {
        var fallbackMapping = BuildFallbackMapping(ctx);

        var enumMemberMappings = ctx.Configuration.Enum.IgnoreCase
            ? BuildEnumMemberMappings(
                ctx,
                ignoreExplicitAndIgnoredMappings: false,
                static x => x.Name,
                StringComparer.Ordinal,
                StringComparer.OrdinalIgnoreCase
            )
            : BuildEnumMemberMappings(ctx, ignoreExplicitAndIgnoredMappings: false, static x => x.Name, StringComparer.Ordinal);

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
        var (ignoredSourceMembers, ignoredTargetMembers, explicitMappings) = GetMappingConfiguration(ctx, ignoreExplicitAndIgnoredMappings);

        var sourceMembers = ctx.SymbolAccessor.GetFieldsExcept(ctx.Source, ignoredSourceMembers);
        var targetMembers = ctx.SymbolAccessor.GetFieldsExcept(ctx.Target, ignoredTargetMembers);

        var targetMembersByProperty = BuildTargetMemberLookups(targetMembers, propertySelector, propertyComparer);

        var (mappings, mappedTargetMembers) = BuildMemberMappingsDictionary(
            sourceMembers,
            targetMembersByProperty,
            explicitMappings,
            propertySelector
        );

        ReportMappingDiagnostics(
            ctx,
            mappings,
            mappedTargetMembers,
            sourceMembers,
            targetMembers,
            ignoredSourceMembers,
            ignoredTargetMembers
        );

        return new EnumMemberMappings(mappings, explicitMappings, targetMembers);
    }

    private static (
        HashSet<IFieldSymbol> ignoredSource,
        HashSet<IFieldSymbol> ignoredTarget,
        Dictionary<IFieldSymbol, IFieldSymbol> explicitMappings
    ) GetMappingConfiguration(MappingBuilderContext ctx, bool ignoreAll)
    {
        if (ignoreAll)
        {
            return (
                new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default),
                new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default),
                new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default)
            );
        }

        return (
            ctx.Configuration.Enum.IgnoredSourceMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault),
            ctx.Configuration.Enum.IgnoredTargetMembers.ToHashSet(SymbolTypeEqualityComparer.FieldDefault),
            BuildExplicitValueMappings(ctx)
        );
    }

    private static List<Dictionary<T, IFieldSymbol>> BuildTargetMemberLookups<T>(
        IReadOnlyCollection<IFieldSymbol> targetMembers,
        Func<IFieldSymbol, T> propertySelector,
        IEqualityComparer<T>[] comparers
    )
        where T : notnull
    {
        return comparers.Select(pc => targetMembers.DistinctBy(propertySelector, pc).ToDictionary(propertySelector, x => x, pc)).ToList();
    }

    private static (Dictionary<IFieldSymbol, IFieldSymbol> mappings, HashSet<IFieldSymbol> mappedTargets) BuildMemberMappingsDictionary<T>(
        IReadOnlyCollection<IFieldSymbol> sourceMembers,
        List<Dictionary<T, IFieldSymbol>> targetMembersByProperty,
        Dictionary<IFieldSymbol, IFieldSymbol> explicitMappings,
        Func<IFieldSymbol, T> propertySelector
    )
        where T : notnull
    {
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

                if (targetMember is null)
                    continue;
            }

            mappings.Add(sourceMember, targetMember);
            mappedTargetMembers.Add(targetMember);
        }

        return (mappings, mappedTargetMembers);
    }

    private static void ReportMappingDiagnostics(
        MappingBuilderContext ctx,
        Dictionary<IFieldSymbol, IFieldSymbol> mappings,
        HashSet<IFieldSymbol> mappedTargetMembers,
        IReadOnlyCollection<IFieldSymbol> sourceMembers,
        IReadOnlyCollection<IFieldSymbol> targetMembers,
        HashSet<IFieldSymbol> ignoredSourceMembers,
        HashSet<IFieldSymbol> ignoredTargetMembers
    )
    {
        EnumMappingDiagnosticReporter.AddUnmappedSourceMembersDiagnostics(ctx, mappings.Keys.ToHashSet(), sourceMembers);
        EnumMappingDiagnosticReporter.AddUnmappedTargetMembersDiagnostics(ctx, mappedTargetMembers, targetMembers);
        EnumMappingDiagnosticReporter.AddUnmatchedSourceIgnoredMembers(ctx, ignoredSourceMembers);
        EnumMappingDiagnosticReporter.AddUnmatchedTargetIgnoredMembers(ctx, ignoredTargetMembers);
    }

    #endregion

    #region Explicit Mappings and Fallback

    private static Dictionary<IFieldSymbol, IFieldSymbol> BuildExplicitValueMappings(MappingBuilderContext ctx)
    {
        var explicitMappings = new Dictionary<IFieldSymbol, IFieldSymbol>(SymbolEqualityComparer.Default);
        var sourceFields = ctx.SymbolAccessor.GetEnumFieldsByValue(ctx.Source);
        var targetFields = ctx.SymbolAccessor.GetEnumFieldsByValue(ctx.Target);

        foreach (var (source, target) in ctx.Configuration.Enum.ExplicitMappings)
        {
            if (!ValidateExplicitMappingTypes(ctx, source, target, ctx.Source, ctx.Target))
                continue;

            if (
                !TryGetMappingFields(
                    sourceFields,
                    targetFields,
                    source.ConstantValue,
                    target.ConstantValue,
                    out var sourceField,
                    out var targetField
                )
            )
                continue;

            if (!explicitMappings.TryAdd(sourceField, targetField))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.EnumSourceValueDuplicated, sourceField, ctx.Source, ctx.Target);
            }
        }

        return explicitMappings;
    }

    private static bool ValidateExplicitMappingTypes(
        MappingBuilderContext ctx,
        AttributeValue source,
        AttributeValue target,
        ITypeSymbol sourceType,
        ITypeSymbol targetType
    )
    {
        if (source.ConstantValue.Kind is not TypedConstantKind.Enum)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.MapValueTypeMismatch,
                source.Expression.ToFullString(),
                source.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                sourceType
            );
            return false;
        }

        if (target.ConstantValue.Kind is not TypedConstantKind.Enum)
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.MapValueTypeMismatch,
                target.Expression.ToFullString(),
                target.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                targetType
            );
            return false;
        }

        if (!SymbolEqualityComparer.Default.Equals(source.ConstantValue.Type, sourceType))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.SourceEnumValueDoesNotMatchSourceEnumType,
                target.Expression.ToFullString(),
                target.ConstantValue.Value ?? 0,
                target.ConstantValue.Type?.ToDisplayString() ?? "unknown",
                sourceType
            );
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(target.ConstantValue.Type, targetType))
        {
            return true;
        }

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.TargetEnumValueDoesNotMatchTargetEnumType,
            source.Expression.ToFullString(),
            source.ConstantValue.Value ?? 0,
            source.ConstantValue.Type?.ToDisplayString() ?? "unknown",
            targetType
        );

        return false;
    }

    private static bool TryGetMappingFields(
        IReadOnlyDictionary<object, IFieldSymbol> sourceFields,
        IReadOnlyDictionary<object, IFieldSymbol> targetFields,
        TypedConstant sourceConstant,
        TypedConstant targetConstant,
        [MaybeNullWhen(false)] out IFieldSymbol sourceField,
        [MaybeNullWhen(false)] out IFieldSymbol targetField
    )
    {
        sourceField = null;
        targetField = null;

        if (sourceConstant.Value is null || targetConstant.Value is null)
        {
            return false;
        }

        return sourceFields.TryGetValue(sourceConstant.Value, out sourceField)
            && targetFields.TryGetValue(targetConstant.Value, out targetField);
    }

    private static EnumFallbackValueMapping BuildFallbackMapping(MappingBuilderContext ctx) => BuildFallbackMapping(ctx, ctx.Target);

    private static EnumFallbackValueMapping BuildFallbackMapping(MappingBuilderContext ctx, ITypeSymbol targetEnumType)
    {
        var fallbackValue = ctx.Configuration.Enum.FallbackValue;
        if (fallbackValue is null)
        {
            return new EnumFallbackValueMapping(ctx.Source, targetEnumType);
        }

        if (fallbackValue is not { Expression: MemberAccessExpressionSyntax memberAccessExpression })
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidEnumMappingFallbackValue, fallbackValue.Value.Expression.ToFullString());
            return new EnumFallbackValueMapping(ctx.Source, targetEnumType);
        }

        if (!SymbolEqualityComparer.Default.Equals(targetEnumType, fallbackValue.Value.ConstantValue.Type))
        {
            ctx.ReportDiagnostic(
                DiagnosticDescriptors.EnumFallbackValueTypeDoesNotMatchTargetEnumType,
                fallbackValue,
                fallbackValue.Value.ConstantValue.Value ?? 0,
                fallbackValue.Value.ConstantValue.Type?.Name ?? "unknown",
                targetEnumType
            );
            return new EnumFallbackValueMapping(ctx.Source, targetEnumType);
        }

        var fallbackExpression = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            FullyQualifiedIdentifier(targetEnumType),
            memberAccessExpression.Name
        );

        return new EnumFallbackValueMapping(ctx.Source, targetEnumType, fallbackExpression: fallbackExpression);
    }

    #endregion

    private sealed record EnumMemberMappings(
        IReadOnlyDictionary<IFieldSymbol, IFieldSymbol> MemberMappings,
        Dictionary<IFieldSymbol, IFieldSymbol> ExplicitMemberMappings,
        IReadOnlyCollection<IFieldSymbol> TargetMembers
    );
}
