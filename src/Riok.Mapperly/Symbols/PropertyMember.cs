using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols;

internal class PropertyMember(IPropertySymbol propertySymbol, SymbolAccessor symbolAccessor) : IMappableMember
{
    private readonly IPropertySymbol _propertySymbol = propertySymbol;

    public string Name => _propertySymbol.Name;
    public ITypeSymbol Type { get; } = symbolAccessor.UpgradeNullable(propertySymbol.Type);

    public ISymbol MemberSymbol => _propertySymbol;
    public bool IsNullable => Type.IsNullable();
    public bool IsIndexer => _propertySymbol.IsIndexer;
    public bool CanGet =>
        !_propertySymbol.IsWriteOnly && (_propertySymbol.GetMethod == null || symbolAccessor.IsAccessible(_propertySymbol.GetMethod));
    public bool CanSet =>
        !_propertySymbol.IsReadOnly && (_propertySymbol.SetMethod == null || symbolAccessor.IsAccessible(_propertySymbol.SetMethod));

    public bool CanSetDirectly =>
        !_propertySymbol.IsReadOnly
        && (_propertySymbol.SetMethod == null || symbolAccessor.IsDirectlyAccessible(_propertySymbol.SetMethod));

    public bool IsInitOnly => _propertySymbol.SetMethod?.IsInitOnly == true;

    public bool IsRequired
#if ROSLYN4_4_OR_GREATER
        => _propertySymbol.IsRequired;
#else
        => false;
#endif

    public ExpressionSyntax BuildAccess(ExpressionSyntax source, bool nullConditional = false)
    {
        return nullConditional ? ConditionalAccess(source, Name) : MemberAccess(source, Name);
    }

    public override bool Equals(object? obj) =>
        obj is PropertyMember other && SymbolEqualityComparer.IncludeNullability.Equals(_propertySymbol, other._propertySymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(_propertySymbol);
}
