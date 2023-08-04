using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public class FieldMember : IMappableMember
{
    private readonly IFieldSymbol _fieldSymbol;

    public FieldMember(IFieldSymbol fieldSymbol)
    {
        _fieldSymbol = fieldSymbol;
    }

    public string Name => _fieldSymbol.Name;
    public ITypeSymbol Type => _fieldSymbol.Type;
    public ISymbol MemberSymbol => _fieldSymbol;
    public bool IsNullable => _fieldSymbol.NullableAnnotation == NullableAnnotation.Annotated || Type.IsNullable();
    public bool IsIndexer => false;
    public bool CanGet => !_fieldSymbol.IsReadOnly;
    public bool CanSet => true;
    public bool IsInitOnly => false;

    public bool IsRequired => _fieldSymbol.IsRequired();

    public override bool Equals(object? obj) =>
        obj is FieldMember other && SymbolEqualityComparer.IncludeNullability.Equals(_fieldSymbol, other._fieldSymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(_fieldSymbol);
}
