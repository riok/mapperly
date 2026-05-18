using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.UnsafeAccess;

namespace Riok.Mapperly.Symbols.Members;

/// <summary>
/// Abstract pseudo-member representing a collection operation in a member path.
/// Subclasses are not real type members — they signal to <see cref="MemberPathGetter"/>
/// that it should emit a LINQ Select() to access the collection element type.
/// </summary>
internal sealed class CollectionElementMember(ITypeSymbol type, ITypeSymbol? typeSymbol) : IMappableMember, IMemberGetter
{
    public string Name => "[]";
    public ITypeSymbol Type { get; } = typeSymbol ?? type;
    public ITypeSymbol CollectionType { get; } = type;
    public INamedTypeSymbol? ContainingType => null;
    public bool IsReadNullable => Type.NullableAnnotation == NullableAnnotation.Annotated;
    public bool IsWriteNullable => false;
    public bool CanGet => true;
    public bool CanGetDirectly => true;
    public bool CanSet => false;
    public bool CanSetDirectly => false;
    public bool IsInitOnly => false;
    public bool IsRequired => false;
    public bool IsObsolete => false;

    public bool IsIgnored(MappingBuilderContext ctx) => false;

    public IMemberGetter BuildGetter(UnsafeAccessorContext ctx) => this;

    public IMemberSetter BuildSetter(UnsafeAccessorContext ctx) =>
        throw new InvalidOperationException($"{GetType().Name} cannot be used as a mapping setter.");

    /// <summary>
    /// Not called directly <see cref="MemberPathGetter"/> handles collection members specially,
    /// emitting a LINQ Select() rather than direct member access.
    /// </summary>
    public ExpressionSyntax BuildAccess(
        ExpressionSyntax? baseAccess,
        INamedTypeSymbol? containingType = null,
        bool nullConditional = false
    ) => throw new InvalidOperationException($"{GetType().Name} must be handled by {nameof(MemberPathGetter)}, not called directly.");
}
