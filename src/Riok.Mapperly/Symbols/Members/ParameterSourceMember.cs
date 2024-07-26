using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.UnsafeAccess;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Symbols.Members;

/// <summary>
/// A mapping method parameter represented as a mappable member.
/// This is semantically not really a member, but it acts as an additional mapping source member
/// and is therefore in terms of the mapping the same.
/// </summary>
[DebuggerDisplay("{Name}")]
public class ParameterSourceMember(MethodParameter parameter) : IMappableMember, IMemberGetter
{
    public string Name => parameter.Name;
    public ITypeSymbol Type => parameter.Type;
    public bool IsNullable => parameter.Type.IsNullable();
    public bool CanGet => true;
    public bool CanGetDirectly => true;
    public bool CanSet => false;
    public bool CanSetDirectly => false;
    public bool IsInitOnly => false;
    public bool IsRequired => false;
    public bool IsObsolete => false;
    public bool IsIgnored => false;

    public IMemberGetter BuildGetter(UnsafeAccessorContext ctx) => this;

    public IMemberSetter BuildSetter(UnsafeAccessorContext ctx) =>
        throw new InvalidOperationException($"Cannot create a setter for {nameof(ParameterSourceMember)}");

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, bool nullConditional = false) => IdentifierName(Name);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (ParameterSourceMember)obj;
        return string.Equals(Name, other.Name, StringComparison.Ordinal)
            && SymbolEqualityComparer.IncludeNullability.Equals(Type, other.Type);
    }

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
}
