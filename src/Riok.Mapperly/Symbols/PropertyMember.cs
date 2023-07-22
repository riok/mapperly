using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

internal class PropertyMember : IMappableMember
{
    private readonly IPropertySymbol _propertySymbol;

    internal PropertyMember(IPropertySymbol propertySymbol)
    {
        _propertySymbol = propertySymbol;
    }

    public string Name => _propertySymbol.Name;
    public ITypeSymbol Type => _propertySymbol.Type;
    public ISymbol MemberSymbol => _propertySymbol;
    public bool IsNullable => _propertySymbol.NullableAnnotation == NullableAnnotation.Annotated || Type.IsNullable();
    public bool IsIndexer => _propertySymbol.IsIndexer;
    public bool CanGet => !_propertySymbol.IsWriteOnly && _propertySymbol.GetMethod?.IsAccessible() != false;
    public bool CanSet => !_propertySymbol.IsReadOnly && _propertySymbol.SetMethod?.IsAccessible() != false;
    public bool IsInitOnly => _propertySymbol.SetMethod?.IsInitOnly == true;

    public bool IsRequired
#if ROSLYN4_4_OR_GREATER
        => _propertySymbol.IsRequired;
#else
        => false;
#endif

    public override bool Equals(object obj) =>
        obj is PropertyMember other && SymbolEqualityComparer.IncludeNullability.Equals(_propertySymbol, other._propertySymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(_propertySymbol);
}
