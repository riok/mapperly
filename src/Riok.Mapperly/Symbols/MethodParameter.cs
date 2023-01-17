using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public readonly struct MethodParameter
{
    public MethodParameter(int ordinal, string name, ITypeSymbol type, RefKind refKind = RefKind.None)
    {
        Ordinal = ordinal;
        Name = name;
        Type = type;
        RefKind = refKind;
    }

    public MethodParameter(IParameterSymbol symbol)
        : this(symbol.Ordinal, symbol.Name, symbol.Type.UpgradeNullable())
    {
    }

    public int Ordinal { get; }

    public string Name { get; }

    public ITypeSymbol Type { get; }

    public RefKind RefKind { get; }

    public MethodArgument WithArgument(ExpressionSyntax? argument)
        => new(this, argument ?? throw new ArgumentNullException(nameof(argument)));

    public static MethodParameter? Wrap(IParameterSymbol? symbol)
        => symbol == null ? null : new(symbol);
}
