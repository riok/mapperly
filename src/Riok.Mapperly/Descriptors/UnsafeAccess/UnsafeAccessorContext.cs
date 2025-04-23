using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

public class UnsafeAccessorContext(UniqueNameBuilder nameBuilder, SymbolAccessor symbolAccessor) : IUnsafeAccessors
{
    private readonly Dictionary<ITypeSymbol, UnsafeAccessorTypeContext> _typeContexts = new(SymbolEqualityComparer.Default);

    // one scope for all type context accessors
    private readonly UniqueNameBuilder _nameBuilder = nameBuilder.NewScope();

    public int Count => _typeContexts.Count;

    public UnsafeSetPropertyAccessor GetOrBuildPropertySetter(PropertyMember member)
    {
        var ctx = GetCtx(member.ContainingType!);
        return ctx.GetOrBuildPropertySetter(member);
    }

    public UnsafeGetPropertyAccessor GetOrBuildPropertyGetter(PropertyMember member)
    {
        var ctx = GetCtx(member.ContainingType!);
        return ctx.GetOrBuildPropertyGetter(member);
    }

    public UnsafeFieldAccessor GetOrBuildFieldGetter(FieldMember member)
    {
        var ctx = GetCtx(member.ContainingType);
        return ctx.GetOrBuildFieldGetter(member);
    }

    public UnsafeConstructorAccessor GetOrBuildConstructor(IMethodSymbol ctor)
    {
        var ctx = GetCtx(ctor.ContainingType!);
        return ctx.GetOrBuildConstructor(ctor);
    }

    public IEnumerable<MemberDeclarationSyntax> Build(SourceEmitterContext ctx, CancellationToken cancellationToken)
    {
        foreach (var accessorCtx in _typeContexts.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return accessorCtx.BuildSyntax(ctx, cancellationToken);
        }
    }

    private UnsafeAccessorTypeContext GetCtx(INamedTypeSymbol type)
    {
        type = type.OriginalDefinition;
        if (_typeContexts.TryGetValue(type, out var ctx))
            return ctx;

        return _typeContexts[type] = new UnsafeAccessorTypeContext(_nameBuilder, type, symbolAccessor);
    }
}
