using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A user defined / implemented mapping.
/// </summary>
public interface IUserMapping : ITypeMapping
{
    IMethodSymbol Method { get; }

    /// <inheritdoc cref="UserMappingAttribute.Default"/>
    bool? Default { get; }

    /// <summary>
    /// An external mapping is defined in another class.
    /// E.g. base class or imported via <see cref="UseStaticMapperAttribute{T}"/>
    /// </summary>
    bool IsExternal { get; }
}
