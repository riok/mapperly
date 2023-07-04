namespace Riok.Mapperly.Descriptors.Enumerables;

[Flags]
public enum CollectionType
{
    None = 0,
    Array = 1 << 0,
    IEnumerable = 1 << 1,

    // collections
    List = 1 << 2,
    Stack = 1 << 3,
    Queue = 1 << 4,
    IReadOnlyCollection = 1 << 5,
    IList = 1 << 6,
    IReadOnlyList = 1 << 7,
    ICollection = 1 << 8,

    // sets
    HashSet = 1 << 9,
    SortedSet = 1 << 10,
    IReadOnlySet = 1 << 11,
    ISet = 1 << 12,

    // dictionaries
    IDictionary = 1 << 13,
    IReadOnlyDictionary = 1 << 14,
    Dictionary = 1 << 15,

    // immutable
    ImmutableArray = 1 << 16,
    ImmutableList = 1 << 17,
    IImmutableList = 1 << 18,
    ImmutableHashSet = 1 << 19,
    IImmutableSet = 1 << 20,
    ImmutableSortedSet = 1 << 21,
    ImmutableQueue = 1 << 22,
    IImmutableQueue = 1 << 23,
    ImmutableStack = 1 << 24,
    IImmutableStack = 1 << 25,
    ImmutableDictionary = 1 << 26,
    IImmutableDictionary = 1 << 27,

    Span = 1 << 28,
    ReadOnlySpan = 1 << 29,
    Memory = 1 << 30,
    ReadOnlyMemory = 1 << 31,
}
