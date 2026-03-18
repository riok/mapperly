using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// A mapping that accepts additional source parameters beyond the source object.
/// </summary>
public interface IParameterizedMapping
{
    IReadOnlyCollection<MethodParameter> AdditionalSourceParameters { get; }
}
