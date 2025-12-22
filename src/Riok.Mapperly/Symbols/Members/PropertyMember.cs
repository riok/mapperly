using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.UnsafeAccess;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols.Members;

[DebuggerDisplay("{Name}")]
public class PropertyMember(IPropertySymbol symbol, SymbolAccessor symbolAccessor)
    : SymbolMappableMember<IPropertySymbol>(symbol),
        IMappableMember,
        IMemberSetter,
        IMemberGetter
{
    public ITypeSymbol Type { get; } = symbolAccessor.UpgradeNullable(symbol.Type);

    public INamedTypeSymbol? ContainingType { get; } = symbol.ContainingType;

    public bool IsNullable => Type.IsNullable();

    public bool CanGet => !Symbol.IsWriteOnly && (Symbol.GetMethod == null || symbolAccessor.IsMemberAccessible(Symbol.GetMethod));

    public bool CanGetDirectly =>
        Symbol is { IsWriteOnly: false, GetMethod: not null } && symbolAccessor.IsDirectlyAccessible(Symbol.GetMethod);

    public bool CanSet => Symbol is { IsReadOnly: false, SetMethod: not null } && symbolAccessor.IsMemberAccessible(Symbol.SetMethod);

    public bool CanSetDirectly =>
        Symbol is { IsReadOnly: false, SetMethod: not null } && symbolAccessor.IsDirectlyAccessible(Symbol.SetMethod);

    public bool IsInitOnly => Symbol.SetMethod?.IsInitOnly == true;

    public bool IsRequired => Symbol.IsRequired;

    public bool IsObsolete => symbolAccessor.HasAttribute<ObsoleteAttribute>(Symbol);
    public bool IsIgnored => symbolAccessor.HasAttribute<MapperIgnoreAttribute>(Symbol);

    public bool SupportsCoalesceAssignment => CanSetDirectly;

    public IMemberGetter BuildGetter(UnsafeAccessorContext ctx)
    {
        if (CanGetDirectly)
            return this;

        if (!CanGet)
            throw new InvalidOperationException($"Cannot build a getter for a property with {nameof(CanGet)} = false");

        return ctx.GetOrBuildPropertyGetter(this);
    }

    public IMemberSetter BuildSetter(UnsafeAccessorContext ctx)
    {
        if (CanSetDirectly)
            return this;

        if (!CanSet)
            throw new InvalidOperationException($"Cannot build a setter for a property with {nameof(CanSet)} = false");

        return ctx.GetOrBuildPropertySetter(this);
    }

    public ExpressionSyntax BuildAssignment(ExpressionSyntax? baseAccess, ExpressionSyntax valueToAssign, bool coalesceAssignment = false)
    {
        Debug.Assert(CanSetDirectly);
        ExpressionSyntax targetMember = baseAccess == null ? IdentifierName(Name) : MemberAccess(baseAccess, Name);

        return Assignment(targetMember, valueToAssign, coalesceAssignment);
    }

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, bool nullConditional = false)
    {
        Debug.Assert(CanGetDirectly);
        if (baseAccess == null)
            return IdentifierName(Name);

        return nullConditional ? ConditionalAccess(baseAccess, Name) : MemberAccess(baseAccess, Name);
    }
}
