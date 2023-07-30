using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public class ParameterMember : IMappableMember
{
    private readonly ITypeSymbol _typeSymbol;

    public ParameterMember(ITypeSymbol typeSymbol, string name)
    {
        _typeSymbol = typeSymbol;
        Name = name;
    }

    public string Name { get; }

    public ITypeSymbol Type => _typeSymbol;

    // TODO: is this okay?
    public ISymbol MemberSymbol => _typeSymbol;

    public bool IsNullable => Type.NullableAnnotation == NullableAnnotation.Annotated || Type.IsNullable();
    public bool IsIndexer => false;
    public bool CanGet => true;
    public bool CanSet => false;
    public bool IsInitOnly => false;

    public bool IsRequired => false;

    public override bool Equals(object obj) =>
        obj is ParameterMember other && SymbolEqualityComparer.IncludeNullability.Equals(Type, other.Type);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(Type);
}
