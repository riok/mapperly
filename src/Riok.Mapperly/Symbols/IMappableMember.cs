using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols;

// TODO comment
public interface IMappableMember
{
    string Name { get; }

    ITypeSymbol Type { get; }

    bool IsNullable { get; }

    bool IsIndexer { get; }

    bool CanGet { get; }

    bool CanSet { get; }

    bool IsInitOnly { get; }

    bool IsRequired { get; }
}
