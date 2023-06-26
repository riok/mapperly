using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public readonly record struct MethodParameter(int Ordinal, string Name, ITypeSymbol Type)
{
    private static readonly IEqualityComparer<ITypeSymbol> _comparer = SymbolEqualityComparer.IncludeNullability;

    public MethodParameter(IParameterSymbol symbol)
        : this(symbol.Ordinal, symbol.Name, symbol.Type.UpgradeNullable()) { }

    public MethodArgument WithArgument(ExpressionSyntax? argument) =>
        new(this, argument ?? throw new ArgumentNullException(nameof(argument)));

    public static MethodParameter? Wrap(IParameterSymbol? symbol) => symbol == null ? null : new(symbol);

    // TODO: this is hacky, adds 10 otherwise ParameterList deletes parameters due to distinctBy
    public static MethodParameter? Wrap10(IParameterSymbol? symbol, int offset) =>
        symbol == null ? null : new(symbol.Ordinal + 10, symbol.Name, symbol.Type.UpgradeNullable());

    public static bool Equals(MethodParameter[] left, MethodParameter[] right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            var l = left[i];
            var r = right[i];
            if (r.Name != l.Name)
                return false;

            if (_comparer.Equals(l.Type, r.Type))
                return false;
        }

        return true;
    }

    public static bool MappableTo(MethodParameter[] current, MethodParameter[] target)
    {
        if (current.Length < target.Length)
        {
            return false;
        }

        var pos = 0;
        foreach (var t in target)
        {
            if (pos >= current.Length)
                return false;

            var tar = t;
            for (var k = pos; k < current.Length; k++)
            {
                var l = current[k];
                if (tar.Name != l.Name)
                    continue;

                if (_comparer.Equals(l.Type, tar.Type))
                {
                    pos = k + 1;
                }
            }
        }

        return pos <= current.Length;
    }
}
