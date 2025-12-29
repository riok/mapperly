using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;

[assembly: UseStaticMapper(typeof(GlobalMappers))]

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static class GlobalMappers
    {
        public static GloballyMappedDtoItem ToDto(GloballyMappedModelItem obj) => new GloballyMappedDtoItem() { Value = obj.Value + 1 };
    }
}
