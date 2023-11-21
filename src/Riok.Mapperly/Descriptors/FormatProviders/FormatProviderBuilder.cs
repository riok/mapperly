using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.FormatProviders;

public static class FormatProviderBuilder
{
    public static FormatProviderCollection ExtractFormatProviders(SimpleMappingBuilderContext ctx, ITypeSymbol mapperSymbol, bool isStatic)
    {
        var formatProviders = mapperSymbol
            .GetMembers()
            .Where(x => ctx.SymbolAccessor.HasAttribute<FormatProviderAttribute>(x))
            .Select(x => BuildFormatProvider(ctx, x, isStatic))
            .WhereNotNull()
            .ToList();

        var defaultFormatProviderCandidates = formatProviders.Where(x => x.Default).Take(2).ToList();
        if (defaultFormatProviderCandidates.Count > 1)
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.MultipleDefaultFormatProviders, defaultFormatProviderCandidates[1].Symbol);
        }

        var formatProvidersByName = formatProviders.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.Single());
        return new FormatProviderCollection(formatProvidersByName, defaultFormatProviderCandidates.FirstOrDefault());
    }

    private static FormatProvider? BuildFormatProvider(SimpleMappingBuilderContext ctx, ISymbol symbol, bool isStatic)
    {
        var memberSymbol = MappableMember.Create(ctx.SymbolAccessor, symbol);
        if (memberSymbol == null)
            return null;

        if (!memberSymbol.CanGet || symbol.IsStatic != isStatic || !memberSymbol.Type.Implements(ctx.Types.Get<IFormatProvider>()))
        {
            ctx.ReportDiagnostic(DiagnosticDescriptors.InvalidFormatProviderSignature, symbol, symbol.Name);
            return null;
        }

        var attribute = ctx.AttributeAccessor.AccessSingle<FormatProviderAttribute>(symbol);
        return new FormatProvider(symbol.Name, attribute.Default, symbol);
    }
}
