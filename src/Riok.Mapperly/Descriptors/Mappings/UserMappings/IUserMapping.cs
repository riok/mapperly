using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A user defined / implemented mapping.
/// </summary>
public interface IUserMapping : ITypeMapping
{
    IMethodSymbol Method { get; }

    ImmutableEquatableArray<MethodParameter> AdditionalParameters { get; }
}
