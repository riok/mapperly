using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.UnsafeAccess;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Symbols.Members;

/// <summary>
/// A constructor parameter represented as a mappable member.
/// This is semantically not really a member, but it acts as a mapping target
/// and is therefore in terms of the mapping the same.
/// </summary>
[DebuggerDisplay("{Name}")]
public class ConstructorParameterMember(IParameterSymbol symbol, SymbolAccessor accessor)
    : SymbolMappableMember<IParameterSymbol>(symbol),
        IMappableMember,
        IMemberGetter
{
    public ITypeSymbol Type { get; } = accessor.UpgradeNullable(symbol.Type);
    public INamedTypeSymbol ContainingType { get; } = symbol.ContainingType;
    public bool IsNullable => Symbol.NullableAnnotation.IsNullable();
    public bool CanGet => false;
    public bool CanGetDirectly => false;
    public bool CanSet => false;
    public bool CanSetDirectly => false;
    public bool IsInitOnly => true;
    public bool IsRequired => !Symbol.IsOptional;
    public bool IsObsolete => false;
    public bool IsIgnored => false;

    public IMemberGetter BuildGetter(UnsafeAccessorContext ctx) => this;

    public IMemberSetter BuildSetter(UnsafeAccessorContext ctx) =>
        throw new InvalidOperationException($"Cannot create a setter for {nameof(ParameterSourceMember)}");

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, bool nullConditional = false) => IdentifierName(Name);
}
