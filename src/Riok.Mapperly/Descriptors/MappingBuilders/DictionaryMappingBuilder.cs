using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DictionaryMappingBuilder
{
    private const string CountPropertyName = nameof(IDictionary<object, object>.Count);

    public static ITypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (!ctx.IsConversionEnabled(MappingConversionType.Dictionary))
            return null;

        if (BuildKeyValueMapping(ctx) is not var (keyMapping, valueMapping))
            return null;

        // target is of type IDictionary<,>, IReadOnlyDictionary<,> or Dictionary<,>.
        // The constructed type should be Dictionary<,>
        if (IsDictionaryType(ctx, ctx.Target))
        {
            var sourceHasCount = ctx.Source.GetAllMembers(CountPropertyName)
                .OfType<IPropertySymbol>()
                .Any(x => !x.IsStatic && !x.IsIndexer && !x.IsWriteOnly && x.Type.SpecialType == SpecialType.System_Int32);

            var targetDictionarySymbol = ctx.Types.DictionaryT.Construct(keyMapping.TargetType, valueMapping.TargetType);
            ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var dictionaryObjectFactory);
            return new ForEachSetDictionaryMapping(
                ctx.Source,
                ctx.Target,
                keyMapping,
                valueMapping,
                sourceHasCount,
                targetDictionarySymbol,
                dictionaryObjectFactory);
        }

        // the target is not a well known dictionary type
        // it should have a an object factory or a parameterless public ctor
        if (!ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory) && !ctx.Target.HasAccessibleParameterlessConstructor())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        if (!ctx.Target.ImplementsGeneric(ctx.Types.IDictionaryT, out _))
            return null;

        return new ForEachSetDictionaryMapping(
            ctx.Source,
            ctx.Target,
            keyMapping,
            valueMapping,
            false,
            objectFactory: objectFactory);
    }

    public static IExistingTargetMapping? TryBuildExistingTargetMapping(MappingBuilderContext ctx)
    {
        if (!ctx.Target.ImplementsGeneric(ctx.Types.IDictionaryT, out _))
            return null;

        if (BuildKeyValueMapping(ctx) is not var (keyMapping, valueMapping))
            return null;

        return new ForEachSetDictionaryExistingTargetMapping(
            ctx.Source,
            ctx.Target,
            keyMapping,
            valueMapping);
    }

    private static (ITypeMapping, ITypeMapping)? BuildKeyValueMapping(MappingBuilderContext ctx)
    {
        if (GetDictionaryKeyValueTypes(ctx, ctx.Target) is not var (targetKeyType, targetValueType))
            return null;

        if (GetEnumerableKeyValueTypes(ctx, ctx.Source) is not var (sourceKeyType, sourceValueType))
            return null;

        var keyMapping = ctx.FindOrBuildMapping(sourceKeyType, targetKeyType);
        if (keyMapping == null)
            return null;

        var valueMapping = ctx.FindOrBuildMapping(sourceValueType, targetValueType);
        if (valueMapping == null)
            return null;

        return (keyMapping, valueMapping);
    }

    private static bool IsDictionaryType(MappingBuilderContext ctx, ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
            return false;

        return SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.Types.DictionaryT)
            || SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.Types.IDictionaryT)
            || SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.Types.IReadOnlyDictionaryT);
    }

    private static (ITypeSymbol, ITypeSymbol)? GetDictionaryKeyValueTypes(MappingBuilderContext ctx, ITypeSymbol t)
    {
        if (t.ImplementsGeneric(ctx.Types.IDictionaryT, out var dictionaryImpl))
        {
            return (dictionaryImpl.TypeArguments[0], dictionaryImpl.TypeArguments[1]);
        }

        if (t.ImplementsGeneric(ctx.Types.IReadOnlyDictionaryT, out var readOnlyDictionaryImpl))
        {
            return (readOnlyDictionaryImpl.TypeArguments[0], readOnlyDictionaryImpl.TypeArguments[1]);
        }

        return null;
    }

    private static (ITypeSymbol, ITypeSymbol)? GetEnumerableKeyValueTypes(MappingBuilderContext ctx, ITypeSymbol t)
    {
        if (!t.ImplementsGeneric(ctx.Types.IEnumerableT, out var enumerableImpl))
            return null;

        if (enumerableImpl.TypeArguments[0] is not INamedTypeSymbol enumeratedType)
            return null;

        if (!SymbolEqualityComparer.Default.Equals(enumeratedType.ConstructedFrom, ctx.Types.KeyValuePairT))
            return null;

        return (enumeratedType.TypeArguments[0], enumeratedType.TypeArguments[1]);
    }
}
