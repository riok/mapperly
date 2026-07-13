using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public partial class MappingTargetOriginalValueMapper
    {
        public partial OptionalDto MapToDto(OptionalObject source);

        public partial void UpdateDto([MappingTarget] OptionalDto dto, OptionalObject source);

        private static string? FromOptional(Optional<string> source, [MappingTargetOriginalValue] string? original) =>
            source.HasValue ? source.Value : original;
    }
}
