using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public record MapperRequiredMappingConfiguration(RequiredMappingStrategy RequiredMappingStrategy)
    : IReversible<MapperRequiredMappingConfiguration>
{
    /// <inheritdoc />
    public MapperRequiredMappingConfiguration Reverse()
    {
        if (RequiredMappingStrategy is RequiredMappingStrategy.None or RequiredMappingStrategy.Both)
            return this;

        var result = RequiredMappingStrategy.None;

        if (RequiredMappingStrategy.HasFlag(RequiredMappingStrategy.Source))
            result |= RequiredMappingStrategy.Target;

        if (RequiredMappingStrategy.HasFlag(RequiredMappingStrategy.Target))
            result |= RequiredMappingStrategy.Source;

        return new MapperRequiredMappingConfiguration(result);
    }
}
