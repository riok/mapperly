using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilder;

public static class DictionaryMappingBuilder
{
    private static readonly string _enumerableIntfName = typeof(IEnumerable<>).FullName;
    private static readonly string _dictionaryClassName = typeof(Dictionary<,>).FullName;
    private static readonly string _dictionaryIntfName = typeof(IDictionary<,>).FullName;
    private static readonly string _readOnlyDictionaryIntfName = typeof(IReadOnlyDictionary<,>).FullName;
    private static readonly string _keyValuePairName = typeof(KeyValuePair<,>).FullName;
    private static readonly string _countPropertyName = "Count";

    public static TypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (ctx.Compilation.GetTypeByMetadataName(_readOnlyDictionaryIntfName) is not { } readOnlyDictionaryIntfSymbol)
            return null;

        if (ctx.Compilation.GetTypeByMetadataName(_dictionaryIntfName) is not { } dictionaryIntfSymbol)
            return null;

        if (GetDictionaryKeyValueTypes(ctx.Target, dictionaryIntfSymbol, readOnlyDictionaryIntfSymbol) is not var (targetKeyType, targetValueType))
            return null;

        if (GetEnumerableKeyValueTypes(ctx, ctx.Source) is not var (sourceKeyType, sourceValueType))
            return null;

        var keyMapping = ctx.FindOrBuildMapping(sourceKeyType, targetKeyType);
        if (keyMapping == null)
            return null;

        var valueMapping = ctx.FindOrBuildMapping(sourceValueType, targetValueType);
        if (valueMapping == null)
            return null;

        // target is of type IDictionary<,> or IReadOnlyDictionary<,>. The constructed type should be Dictionary<,>
        if (ctx.Compilation.GetTypeByMetadataName(_dictionaryClassName) is { } dictionaryClassSymbol
            && (IsDictionaryInterface(ctx.Target, dictionaryIntfSymbol, readOnlyDictionaryIntfSymbol) || ctx.Target.ImplementsGeneric(dictionaryClassSymbol, out _)))
        {
            var sourceHasCount = ctx.Source.GetAllMembers(_countPropertyName)
                .OfType<IPropertySymbol>()
                .Any(x => !x.IsStatic && !x.IsIndexer && !x.IsWriteOnly && x.Type.SpecialType == SpecialType.System_Int32);

            var targetDictionarySymbol = dictionaryClassSymbol.Construct(targetKeyType, targetValueType);
            return new ForEachAddDictionaryMapping(ctx.Source, ctx.Target, keyMapping, valueMapping, sourceHasCount, targetDictionarySymbol);
        }

        // the target is not a well known dictionary type
        // it should have a parameterless public ctor
        if (!ctx.Target.HasAccessibleParameterlessConstructor())
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.NoParameterlessConstructorFound, ctx.Target);
            return null;
        }

        return new ForEachAddDictionaryMapping(ctx.Source, ctx.Target, keyMapping, valueMapping, false);
    }

    private static bool IsDictionaryInterface(ITypeSymbol symbol, ISymbol dictionaryIntfSymbol, ISymbol readOnlyDictionaryIntfSymbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
            return false;

        return SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, readOnlyDictionaryIntfSymbol)
            || SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, dictionaryIntfSymbol);
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
        if (ctx.Compilation.GetTypeByMetadataName(_enumerableIntfName) is not { } enumerableSymbol
            || ctx.Compilation.GetTypeByMetadataName(_keyValuePairName) is not { } keyValueSymbol)
        {
            return null;
        }

        if (!t.ImplementsGeneric(enumerableSymbol, out var enumerableImpl))
            return null;

        if (enumerableImpl.TypeArguments[0] is not INamedTypeSymbol enumeratedType)
            return null;

        if (!SymbolEqualityComparer.Default.Equals(enumeratedType.ConstructedFrom, keyValueSymbol))
            return null;

        return (enumeratedType.TypeArguments[0], enumeratedType.TypeArguments[1]);
    }
}
