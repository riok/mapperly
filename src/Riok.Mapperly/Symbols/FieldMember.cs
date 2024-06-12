using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols;

[DebuggerDisplay("{Name}")]
public class FieldMember(IFieldSymbol fieldSymbol, SymbolAccessor symbolAccessor) : IMappableMember
{
    private readonly IFieldSymbol _fieldSymbol = fieldSymbol;

    public string Name => _fieldSymbol.Name;
    public ITypeSymbol Type { get; } = symbolAccessor.UpgradeNullable(fieldSymbol.Type);
    public ISymbol MemberSymbol => _fieldSymbol;
    public bool IsNullable => Type.IsNullable();
    public bool IsIndexer => false;
    public bool CanGet => true;
    public bool CanSet => !_fieldSymbol.IsReadOnly;
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
