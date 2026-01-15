using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.UnsafeAccess;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols.Members;

[DebuggerDisplay("{Name}")]
public class FieldMember(IFieldSymbol symbol, SymbolAccessor symbolAccessor)
    : SymbolMappableMember<IFieldSymbol>(symbol),
        IMappableMember,
        IMemberGetter,
        IMemberSetter
{
    public ITypeSymbol Type { get; } = symbolAccessor.UpgradeNullable(symbol.Type);
    public INamedTypeSymbol ContainingType { get; } = symbol.ContainingType;
    public bool IsNullable => Type.IsNullable();
    public bool CanGet => true;
    public bool CanGetDirectly => symbolAccessor.IsDirectlyAccessible(Symbol);
    public bool CanSet => !Symbol.IsReadOnly;
    public bool CanSetDirectly => CanSet && symbolAccessor.IsDirectlyAccessible(Symbol);
    public bool IsInitOnly => false;

    public bool IsRequired => Symbol.IsRequired;

    public bool IsObsolete => symbolAccessor.HasAttribute<ObsoleteAttribute>(Symbol);
    public bool IsIgnored => symbolAccessor.HasAttribute<MapperIgnoreAttribute>(Symbol);
    public bool SupportsCoalesceAssignment => true;

    public IMemberGetter BuildGetter(UnsafeAccessorContext ctx)
    {
        if (CanGetDirectly)
            return this;

        if (!CanGet)
            throw new InvalidOperationException($"Cannot build a getter for a property with {nameof(CanGet)} = false");

        return ctx.GetOrBuildFieldGetter(this);
    }

    public IMemberSetter BuildSetter(UnsafeAccessorContext ctx)
    {
        if (CanSetDirectly)
            return this;

        if (!CanSet)
            throw new InvalidOperationException($"Cannot build a setter for a property with {nameof(CanSet)} = false");

        return ctx.GetOrBuildFieldGetter(this);
    }

    public ExpressionSyntax BuildAssignment(
        ExpressionSyntax? baseAccess,
        ExpressionSyntax valueToAssign,
        INamedTypeSymbol? containingType = null,
        bool coalesceAssignment = false
    )
    {
        var targetMemberRef = BuildAccess(baseAccess);
        return Assignment(targetMemberRef, valueToAssign, coalesceAssignment);
    }

    public ExpressionSyntax BuildAccess(ExpressionSyntax? baseAccess, INamedTypeSymbol? containingType = null, bool nullConditional = false)
    {
        if (baseAccess == null)
            return SyntaxFactory.IdentifierName(Name);

        return nullConditional ? ConditionalAccess(baseAccess, Name) : MemberAccess(baseAccess, Name);
    }
}
