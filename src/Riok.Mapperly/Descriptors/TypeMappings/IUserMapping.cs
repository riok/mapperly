using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// A user defined / implemented mapping.
/// </summary>
public interface IUserMapping
{
    IMethodSymbol Method { get; }
}
