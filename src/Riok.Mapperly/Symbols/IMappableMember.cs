using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// A mappable member is a member of a class which can take part in a mapping.
/// (eg. a field or a property).
/// </summary>
public interface IMappableMember
{
    string Name { get; }

    ITypeSymbol Type { get; }

    ISymbol MemberSymbol { get; }

    bool IsNullable { get; }

    bool IsIndexer { get; }

    bool CanGet { get; }

    bool CanSet { get; }

    bool IsInitOnly { get; }

    bool IsRequired { get; }
}
