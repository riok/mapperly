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
    public bool IsNullable => _fieldSymbol.NullableAnnotation == NullableAnnotation.Annotated || Type.IsNullable();
    public bool IsIndexer => false;
    public bool CanGet => !_fieldSymbol.IsReadOnly && _fieldSymbol.IsAccessible();
    public bool CanSet => _fieldSymbol.IsAccessible();
    public bool IsInitOnly => false;

    public bool IsRequired
#if ROSLYN4_4_OR_GREATER
        => _fieldSymbol.IsRequired;
#else
        => false;
#endif

    public override bool Equals(object obj) =>
        obj is FieldMember other && SymbolEqualityComparer.IncludeNullability.Equals(_fieldSymbol, other._fieldSymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(_fieldSymbol);
}
