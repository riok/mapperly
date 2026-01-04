using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.UnsafeAccess;

namespace Riok.Mapperly.Symbols.Members;

/// <summary>
/// A mappable member is a member of a class which can take part in a mapping.
/// (e.g., a field or a property).
/// </summary>
public interface IMappableMember
{
    string Name { get; }

    ITypeSymbol Type { get; }

    INamedTypeSymbol? ContainingType { get; }

    bool IsNullable { get; }

    /// <summary>
    /// Whether the member can be read using direct access or an unsafe accessor method.
    /// </summary>
    bool CanGet { get; }

    /// <summary>
    /// Whether the member can be read using simple assignment.
    /// </summary>
    bool CanGetDirectly { get; }

    /// <summary>
    /// Whether the member can be modified using an assignment or an unsafe accessor method.
    /// </summary>
    bool CanSet { get; }

    /// <summary>
    /// Whether the member can be modified using simple assignment.
    /// </summary>
    bool CanSetDirectly { get; }

    bool IsInitOnly { get; }

    bool IsRequired { get; }

    bool IsObsolete { get; }

    /// <summary>
    /// Whether this member is attributed with <see cref="Riok.Mapperly.Abstractions.MapperIgnoreAttribute"/>.
    /// </summary>
    bool IsIgnored { get; }

    bool IsSpecialAdditionalSource { get; }

    IMemberGetter BuildGetter(UnsafeAccessorContext ctx);
    IMemberSetter BuildSetter(UnsafeAccessorContext ctx);
}
