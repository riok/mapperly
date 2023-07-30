using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public readonly struct MethodParameter : IEquatable<MethodParameter>
{
    private static readonly IEqualityComparer<ITypeSymbol> _comparer = SymbolEqualityComparer.IncludeNullability;

    public MethodParameter(IParameterSymbol symbol)
        : this(symbol.Ordinal, symbol.Name, symbol.Type.UpgradeNullable()) { }

    public MethodParameter(int ordinal, string name, ITypeSymbol type)
    {
        Ordinal = ordinal;
        Name = name;
        Type = type;
    }

    public int Ordinal { get; init; }
    public string Name { get; init; }
    public ITypeSymbol Type { get; init; }

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
            if (left[i].Equals(right[i]))
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
                if (StringComparer.Ordinal.Equals(tar.Name, l.Name))
                    continue;

                if (_comparer.Equals(l.Type, tar.Type))
                {
                    pos = k + 1;
                }
            }
        }

        return pos <= current.Length;
    }

    public bool Equals(MethodParameter other) =>
        Ordinal == other.Ordinal
        && StringComparer.Ordinal.Equals(Name, other.Name)
        && SymbolEqualityComparer.Default.Equals(Type, other.Type);

    public override bool Equals(object? obj) => obj is MethodParameter other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Ordinal;
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(Name);
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(Type);
            return hashCode;
        }
    }
}
