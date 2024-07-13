using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Enumerables;

public record CollectionInfo(
    ITypeSymbol Type,
    CollectionType CollectionType,
    CollectionType ImplementedTypes,
    ITypeSymbol EnumeratedType,
    IMappableMember? CountMember,
    string? AddMethodName,
    bool IsImmutableCollectionType
)
{
    public bool ImplementsIEnumerable => ImplementedTypes.HasFlag(CollectionType.IEnumerable);

    public bool IsArray => CollectionType is CollectionType.Array;
    public bool IsMemory => CollectionType is CollectionType.Memory or CollectionType.ReadOnlyMemory;
    public bool IsSpan => CollectionType is CollectionType.Span or CollectionType.ReadOnlySpan;

    [MemberNotNullWhen(true, nameof(CountMember))]
    public bool CountIsKnown => CountMember != null;
}
