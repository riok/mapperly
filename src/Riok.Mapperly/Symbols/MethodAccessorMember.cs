using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols;

public class MethodAccessorMember : IMappableMember
{
    private readonly IMappableMember _mappableMember;
    private readonly string _methodName;

    /// <summary>
    /// This member requires invocation with a parameter.
    /// </summary>
    private readonly bool _methodRequiresParameter;

    public MethodAccessorMember(IMappableMember mappableMember, string methodName, bool methodRequiresParameter = false)
    {
        _mappableMember = mappableMember;
        _methodName = methodName;
        _methodRequiresParameter = methodRequiresParameter;
    }

    public string Name => _mappableMember.Name;
    public ITypeSymbol Type => _mappableMember.Type;
    public ISymbol MemberSymbol => _mappableMember.MemberSymbol;
    public bool IsNullable => _mappableMember.IsNullable;
    public bool IsIndexer => _mappableMember.IsIndexer;
    public bool CanGet => _mappableMember.CanGet;
    public bool CanSet => _mappableMember.CanSet;
    public bool CanSetDirectly => _mappableMember.CanSetDirectly;
    public bool IsInitOnly => _mappableMember.IsInitOnly;
    public bool IsRequired => _mappableMember.IsRequired;

    public ExpressionSyntax BuildAccess(ExpressionSyntax source, bool nullConditional = false)
    {
        if (_methodRequiresParameter)
        {
            // the receiver of the resulting ExpressionSyntax will add an invocation call with a parameter
            // src?.SetValue or src.SetValue
            return nullConditional ? ConditionalAccess(source, _methodName) : MemberAccess(source, _methodName);
        }

        // src?.GetValue() or src.GetValue()
        return nullConditional ? Invocation(ConditionalAccess(source, _methodName)) : Invocation(MemberAccess(source, _methodName));
    }

    public override bool Equals(object? obj) =>
        obj is MethodAccessorMember other
        && SymbolEqualityComparer.IncludeNullability.Equals(_mappableMember.MemberSymbol, other.MemberSymbol);

    public override int GetHashCode() => SymbolEqualityComparer.IncludeNullability.GetHashCode(_mappableMember.MemberSymbol);
}
