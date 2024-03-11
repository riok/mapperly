using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.Enumerables;

public record DictionaryInfo(CollectionInfo Collection, ITypeSymbol Key, ITypeSymbol Value);
