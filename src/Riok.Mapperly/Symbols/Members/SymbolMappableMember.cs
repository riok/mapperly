using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols.Members;

public abstract class SymbolMappableMember<T>(T symbol)
    where T : ISymbol
{
    public T Symbol { get; } = symbol;

    public string Name => Symbol.Name;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (SymbolMappableMember<T>)obj;
        return SymbolEqualityComparer.IncludeNullability.Equals(Symbol, other.Symbol);
    }

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(Symbol);
}
