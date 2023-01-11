using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A user defined / implemented mapping.
/// </summary>
public interface IUserMapping : ITypeMapping
{
    IMethodSymbol Method { get; }
}
