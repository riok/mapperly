using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;

[assembly: UseStaticMapper(typeof(GlobalMappers1))]

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static class GlobalMappers1
    {
        public static GloballyMappedDto1 ToDto(GloballyMappedModel1 obj) => new GloballyMappedDto1() { Value = obj.Value + 1 };
    }
}
