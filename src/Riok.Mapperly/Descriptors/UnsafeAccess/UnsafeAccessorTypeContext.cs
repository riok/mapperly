using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Emit;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.UnsafeAccess;

public class UnsafeAccessorTypeContext(
    UniqueNameBuilder nameBuilder,
    INamedTypeSymbol type,
    SymbolAccessor symbolAccessor,
    AggressiveInliningTypes aggressiveInliningTypes
)
{
    private readonly string _accessorClassName = nameBuilder.New($"{type.Name}Accessor");
    private readonly List<IUnsafeAccessor> _unsafeAccessors = [];
    private readonly Dictionary<UnsafeAccessorKey, IUnsafeAccessor> _unsafeAccessorsBySymbol = new();
    private readonly UniqueNameBuilder _nameBuilder = nameBuilder.NewScope();

    public UnsafeSetPropertyAccessor GetOrBuildPropertySetter(PropertyMember member)
    {
        return GetOrBuild(
            UnsafeAccessorType.SetProperty,
            member.Symbol,
            (m, className, methodName) => new UnsafeSetPropertyAccessor(m, className, methodName, ShouldApplyMethodImpl())
        );
    }

    public UnsafeGetPropertyAccessor GetOrBuildPropertyGetter(PropertyMember member)
    {
        return GetOrBuild(
            UnsafeAccessorType.GetProperty,
            member.Symbol,
            (m, className, methodName) => new UnsafeGetPropertyAccessor(m, className, methodName, ShouldApplyMethodImpl())
        );
    }

    public UnsafeFieldAccessor GetOrBuildFieldGetter(FieldMember member)
    {
        return GetOrBuild(
            UnsafeAccessorType.GetField,
            member.Symbol,
            (m, className, methodName) => new UnsafeFieldAccessor(m, className, methodName, ShouldApplyMethodImpl())
        );
    }

    public UnsafeConstructorAccessor GetOrBuildConstructor(IMethodSymbol ctorSymbol)
    {
        return GetOrBuild(
            UnsafeAccessorType.Constructor,
            ctorSymbol,
            (s, className, methodName) => new UnsafeConstructorAccessor(s, className, methodName, ShouldApplyMethodImpl())
        );
    }

    internal MemberDeclarationSyntax BuildSyntax(SourceEmitterContext ctx, CancellationToken cancellationToken)
    {
#if ROSLYN4_7_OR_GREATER
        var accessorCtx = ctx.AddIndentation();
        var accessors = BuildAccessorsSyntax(accessorCtx, cancellationToken);
        accessors = accessors.SeparateByLineFeed(accessorCtx.SyntaxFactory.Indentation);
        var clazz = ctx.SyntaxFactory.Class(
            _accessorClassName,
            TokenList(TrailingSpacedToken(SyntaxKind.StaticKeyword), TrailingSpacedToken(SyntaxKind.FileKeyword)),
            List(accessors)
        );

        if (type.IsGenericType)
        {
            clazz = ctx.SyntaxFactory.AddTypeParameters(clazz, type.ConstructedFrom);
        }

        return clazz;
#else
        throw new InvalidOperationException("Unsafe accessors are not supported for Roslyn versions < 4.7");
#endif
    }

    private bool ShouldApplyMethodImpl()
    {
        return aggressiveInliningTypes == AggressiveInliningTypes.All;
    }

    private IEnumerable<MemberDeclarationSyntax> BuildAccessorsSyntax(SourceEmitterContext ctx, CancellationToken cancellationToken)
    {
        foreach (var accessor in _unsafeAccessors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return accessor.BuildAccessorMethod(ctx);
        }
    }

    private TAccessor GetOrBuild<TAccessor, TSymbol>(
        UnsafeAccessorType accessorType,
        TSymbol symbol,
        Func<TSymbol, string, string, TAccessor> factory
    )
        where TAccessor : IUnsafeAccessor
        where TSymbol : ISymbol
    {
        var key = new UnsafeAccessorKey(symbol.OriginalDefinition, accessorType);
        if (TryGetAccessor<TAccessor>(key, out var accessor))
            return accessor;

        var methodName = accessorType switch
        {
            UnsafeAccessorType.GetProperty or UnsafeAccessorType.GetField => BuildExtensionMethodName("Get", symbol),
            UnsafeAccessorType.SetProperty => BuildExtensionMethodName("Set", symbol),
            UnsafeAccessorType.Constructor => _nameBuilder.New("Create"),
            _ => throw new ArgumentOutOfRangeException(nameof(accessorType), accessorType, "Unknown type"),
        };

        return CacheAccessor(key, factory(symbol, _accessorClassName, methodName));
    }

    private T CacheAccessor<T>(UnsafeAccessorKey key, T accessor)
        where T : IUnsafeAccessor
    {
        _unsafeAccessorsBySymbol.Add(key, accessor);
        _unsafeAccessors.Add(accessor);
        return accessor;
    }

    private bool TryGetAccessor<T>(UnsafeAccessorKey key, [NotNullWhen(true)] out T? accessor)
        where T : IUnsafeAccessor
    {
        if (_unsafeAccessorsBySymbol.TryGetValue(key, out var acc))
        {
            accessor = (T)acc;
            return true;
        }

        accessor = default;
        return false;
    }

    private string BuildExtensionMethodName(string prefix, ISymbol symbol)
    {
        var methodName = prefix + FormatAccessorName(symbol.Name);
        return GetUniqueMethodName(symbol.ContainingType, methodName);
    }

    private string GetUniqueMethodName(ITypeSymbol symbol, string name)
    {
        var memberNames = symbolAccessor.GetAllMembers(symbol).Select(x => x.Name);
        return _nameBuilder.New(name, memberNames);
    }

    /// <summary>
    /// Strips the leading underscore and capitalise the first letter.
    /// </summary>
    /// <param name="name">Accessor name to be formatted.</param>
    /// <returns>Formatted accessor name.</returns>
    private static string FormatAccessorName(string name)
    {
        name = name.TrimStart('_');
        if (name.Length == 0)
            return name;

        return char.ToUpper(name[0], CultureInfo.InvariantCulture) + name[1..];
    }

    private enum UnsafeAccessorType
    {
        GetProperty,
        SetProperty,
        GetField,
        Constructor,
    }

    private readonly struct UnsafeAccessorKey(ISymbol member, UnsafeAccessorType type) : IEquatable<UnsafeAccessorKey>
    {
        private readonly ISymbol _member = member;
        private readonly UnsafeAccessorType _type = type;

        public bool Equals(UnsafeAccessorKey other) =>
            _type == other._type && SymbolEqualityComparer.Default.Equals(_member, other._member);

        public override bool Equals(object? obj) => obj is UnsafeAccessorKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SymbolEqualityComparer.Default.GetHashCode(_member);
                hashCode = (hashCode * 397) ^ (int)_type;
                return hashCode;
            }
        }
    }
}
