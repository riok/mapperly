using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public readonly record struct MethodParameter(int Ordinal, string Name, ITypeSymbol Type)
{
    public MethodParameter(IParameterSymbol symbol)
        : this(symbol.Ordinal, symbol.Name, symbol.Type.UpgradeNullable()) { }

    public MethodArgument WithArgument(ExpressionSyntax? argument) =>
        new(this, argument ?? throw new ArgumentNullException(nameof(argument)));

    public static MethodParameter? Wrap(IParameterSymbol? symbol) => symbol == null ? null : new(symbol);

    // TODO: this is hacky, adds 10 otherwise ParameterList deletes parameters due to distinctBy
    public static MethodParameter? Wrap10(IParameterSymbol? symbol, int offset) =>
        symbol == null ? null : new(symbol.Ordinal + 10, symbol.Name, symbol.Type.UpgradeNullable());
}
