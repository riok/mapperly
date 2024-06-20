using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// A constructor parameter represented as a mappable member.
/// This is semantically not really a member, but it acts as a mapping target
/// and is therefore in terms of the mapping the same.
/// </summary>
[DebuggerDisplay("{Name}")]
public class ConstructorParameterMember(IParameterSymbol fieldSymbol, SymbolAccessor accessor) : IMappableMember
{
    private readonly IParameterSymbol _fieldSymbol = fieldSymbol;

    public string Name => _fieldSymbol.Name;
    public ITypeSymbol Type { get; } = accessor.UpgradeNullable(fieldSymbol.Type);
    public ISymbol MemberSymbol => _fieldSymbol;
    public bool IsNullable => _fieldSymbol.NullableAnnotation.IsNullable();
    public bool IsIndexer => false;
    public bool CanGet => false;
    public bool CanSet => false;
    public bool CanSetDirectly => false;
    public bool IsInitOnly => true;
    public bool IsRequired => !_fieldSymbol.IsOptional;

    public ExpressionSyntax BuildAccess(ExpressionSyntax source, bool nullConditional = false) =>
        throw new InvalidOperationException("Cannot access a constructor parameter");

    public override bool Equals(object? obj) =>
        obj is ConstructorParameterMember other && SymbolEqualityComparer.IncludeNullability.Equals(_fieldSymbol, other._fieldSymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(_fieldSymbol);
}
