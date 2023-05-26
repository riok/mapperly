using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A user defined / implemented mapping.
/// </summary>
public interface IUserMapping : ITypeMapping
{
    IMethodSymbol Method { get; }
}
