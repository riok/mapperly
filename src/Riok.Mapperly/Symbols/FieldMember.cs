using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

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
    public bool CanSetDirectly => true;
    public bool IsInitOnly => false;

    public bool IsRequired
#if ROSLYN4_4_OR_GREATER
        => _fieldSymbol.IsRequired;
#else
        => false;
#endif

    public ExpressionSyntax BuildAccess(ExpressionSyntax source, bool nullConditional = false)
    {
        return nullConditional ? ConditionalAccess(source, Name) : MemberAccess(source, Name);
    }

    public override bool Equals(object? obj) =>
        obj is FieldMember other && SymbolEqualityComparer.IncludeNullability.Equals(_fieldSymbol, other._fieldSymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(_fieldSymbol);
}
