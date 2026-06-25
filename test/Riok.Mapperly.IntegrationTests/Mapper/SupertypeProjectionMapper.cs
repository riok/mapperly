using System.Linq;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(AutoUserMappings = false)]
    public static partial class SupertypeProjectionMapper
    {
        public static partial IQueryable<SupertypeProjectionDto> ProjectToDto(this IQueryable<SupertypeProjectionSource> source);

        [MapPropertyFromSource(nameof(SupertypeProjectionDto.MappedValue), Use = nameof(MapValue))]
        [MapPropertyFromSource(nameof(SupertypeProjectionDto.MappedName), Use = nameof(MapName))]
        private static partial SupertypeProjectionDto Map(SupertypeProjectionSource source);

        // The parameter types are supertypes (interface / base class) of the projection's source element
        // type, so inlining these helpers inserts upcasts that must stay parenthesized: `((I)x).Value`.
        private static int? MapValue(ISupertypeProjectionValue source) => source.Value;

        private static string MapName(SupertypeProjectionBase source) => source.Name;
    }
}
