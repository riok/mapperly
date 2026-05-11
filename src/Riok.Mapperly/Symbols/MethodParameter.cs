using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Symbols;

public readonly record struct MethodParameter(
    int Ordinal,
    string Name,
    ITypeSymbol Type,
    IParameterSymbol? Symbol = null,
    RefKind RefKind = RefKind.None
)
{
    private static readonly SymbolDisplayFormat _parameterNameFormat = new(
        parameterOptions: SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
    );

    public MethodParameter(IParameterSymbol symbol, ITypeSymbol parameterType)
        : this(symbol.Ordinal, symbol.ToDisplayString(_parameterNameFormat), parameterType, symbol, symbol.RefKind) { }

    /// <summary>
    /// The parameter name with the verbatim identifier prefix (<c>@</c>) removed.
    /// </summary>
    public string NormalizedName => NormalizeName(Name);

    public static string NormalizeName(string name) => name.TrimStart('@');

    public MethodArgument WithArgument(ExpressionSyntax? argument) =>
        new(this, argument ?? throw new ArgumentNullException(nameof(argument)));
}
