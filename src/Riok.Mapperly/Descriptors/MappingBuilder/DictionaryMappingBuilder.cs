using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class DictionaryMappingBuilder
{
    private static readonly string _countPropertyName = "Count";

    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (GetDictionaryKeyValueTypes(ctx.Target, ctx.GetTypeSymbol(typeof(IDictionary<,>)), ctx.GetTypeSymbol(typeof(IReadOnlyDictionary<,>))) is not var (targetKeyType, targetValueType))
            return null;

        if (GetEnumerableKeyValueTypes(ctx, ctx.Source) is not var (sourceKeyType, sourceValueType))
            return null;

        var keyMapping = ctx.FindOrBuildMapping(sourceKeyType, targetKeyType);
        if (keyMapping == null)
            return null;

        var valueMapping = ctx.FindOrBuildMapping(sourceValueType, targetValueType);
        if (valueMapping == null)
            return null;

        // target is of type IDictionary<,> or IReadOnlyDictionary<,>.
        // The constructed type should be Dictionary<,>
        if (IsDictionaryType(ctx, ctx.Target))
        {
            var sourceHasCount = ctx.Source.GetAllMembers(_countPropertyName)
                .OfType<IPropertySymbol>()
                .Any(x => !x.IsStatic && !x.IsIndexer && !x.IsWriteOnly && x.Type.SpecialType == SpecialType.System_Int32);

            var targetDictionarySymbol = ctx.GetTypeSymbol(typeof(Dictionary<,>)).Construct(targetKeyType, targetValueType);
            ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var dictionaryObjectFactory);
            return new ForEachAddDictionaryMapping(ctx.Source, ctx.Target, keyMapping, valueMapping, sourceHasCount, targetDictionarySymbol, dictionaryObjectFactory);
        }

        // the target is not a well known dictionary type
        // it should have a an object factory or a parameterless public ctor
        if (!ctx.ObjectFactories.TryFindObjectFactory(ctx.Source, ctx.Target, out var objectFactory) && !ctx.Target.HasAccessibleParameterlessConstructor())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        return new ForEachAddDictionaryMapping(ctx.Source, ctx.Target, keyMapping, valueMapping, false, objectFactory: objectFactory);
    }

    private static bool IsDictionaryType(MappingBuilderContext ctx, ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
            return false;

        return SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.GetTypeSymbol(typeof(Dictionary<,>)))
            || SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.GetTypeSymbol(typeof(IDictionary<,>)))
            || SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, ctx.GetTypeSymbol(typeof(IReadOnlyDictionary<,>)));
    }

    private static (ITypeSymbol, ITypeSymbol)? GetDictionaryKeyValueTypes(
        ITypeSymbol t,
        INamedTypeSymbol dictionarySymbol,
        INamedTypeSymbol readOnlyDictionarySymbol)
    {
        if (t.ImplementsGeneric(dictionarySymbol, out var dictionaryImpl))
        {
            return (dictionaryImpl.TypeArguments[0], dictionaryImpl.TypeArguments[1]);
        }

        if (t.ImplementsGeneric(readOnlyDictionarySymbol, out var readOnlyDictionaryImpl))
        {
            return (readOnlyDictionaryImpl.TypeArguments[0], readOnlyDictionaryImpl.TypeArguments[1]);
        }

        return null;
    }

    private static (ITypeSymbol, ITypeSymbol)? GetEnumerableKeyValueTypes(MappingBuilderContext ctx, ITypeSymbol t)
    {
        if (!t.ImplementsGeneric(ctx.GetTypeSymbol(typeof(IEnumerable<>)), out var enumerableImpl))
            return null;

        if (enumerableImpl.TypeArguments[0] is not INamedTypeSymbol enumeratedType)
            return null;

        if (!SymbolEqualityComparer.Default.Equals(enumeratedType.ConstructedFrom, ctx.GetTypeSymbol(typeof(KeyValuePair<,>))))
            return null;

        return (enumeratedType.TypeArguments[0], enumeratedType.TypeArguments[1]);
    }
}
