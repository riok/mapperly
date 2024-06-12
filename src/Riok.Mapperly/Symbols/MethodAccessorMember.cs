using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols;

[DebuggerDisplay("Accessor {Name}")]
public class MethodAccessorMember(IMappableMember mappableMember, string methodName, bool methodRequiresParameter = false) : IMappableMember
{
    /// <summary>
    /// This member requires invocation with a parameter.
    /// </summary>
    private readonly bool _methodRequiresParameter = methodRequiresParameter;

    public string Name => mappableMember.Name;
    public ITypeSymbol Type => mappableMember.Type;
    public ISymbol MemberSymbol => mappableMember.MemberSymbol;
    public bool IsNullable => mappableMember.IsNullable;
    public bool IsIndexer => mappableMember.IsIndexer;
    public bool CanGet => mappableMember.CanGet;
    public bool CanSet => mappableMember.CanSet;
    public bool CanSetDirectly => mappableMember.CanSetDirectly;
    public bool IsInitOnly => mappableMember.IsInitOnly;
    public bool IsRequired => mappableMember.IsRequired;

    public ExpressionSyntax BuildAccess(ExpressionSyntax source, bool nullConditional = false)
    {
        if (_methodRequiresParameter)
        {
            // the receiver of the resulting ExpressionSyntax will add an invocation call with a parameter
            // src?.SetValue or src.SetValue
            return nullConditional ? ConditionalAccess(source, methodName) : MemberAccess(source, methodName);
        }

        // src?.GetValue() or src.GetValue()
        return nullConditional ? Invocation(ConditionalAccess(source, methodName)) : Invocation(MemberAccess(source, methodName));
    }

    public override bool Equals(object? obj) =>
        obj is MethodAccessorMember other
        && SymbolEqualityComparer.IncludeNullability.Equals(mappableMember.MemberSymbol, other.MemberSymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(mappableMember.MemberSymbol);
}
