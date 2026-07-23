using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.Constructors;

public interface IParameterMappingInstanceConstructor : IInstanceConstructor
{
    bool SupportsParameterMapping { get; }

    IMethodSymbol ParameterMappingMethod { get; }
}
