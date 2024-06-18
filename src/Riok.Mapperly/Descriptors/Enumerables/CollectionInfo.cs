using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.Enumerables;

public record CollectionInfo(
    ITypeSymbol Type,
    CollectionType CollectionType,
    CollectionType ImplementedTypes,
    ITypeSymbol EnumeratedType,
    string? CountPropertyName,
    bool HasImplicitCollectionAddMethod,
    bool IsImmutableCollectionType
)
{
    public bool ImplementsIEnumerable => ImplementedTypes.HasFlag(CollectionType.IEnumerable);

    public bool ImplementsDictionary =
        ImplementedTypes.HasFlag(CollectionType.IDictionary) || ImplementedTypes.HasFlag(CollectionType.IReadOnlyDictionary);
    public bool IsArray => CollectionType is CollectionType.Array;
    public bool IsMemory => CollectionType is CollectionType.Memory or CollectionType.ReadOnlyMemory;
    public bool IsSpan => CollectionType is CollectionType.Span or CollectionType.ReadOnlySpan;

    [MemberNotNullWhen(true, nameof(CountPropertyName))]
    public bool CountIsKnown => CountPropertyName != null;
}
