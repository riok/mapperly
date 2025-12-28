using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;

[assembly: UseStaticMapper<GlobalMappers2>]

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public class GlobalMappers2
    {
        public static GloballyMappedDto2 ToDto(GloballyMappedModel2 obj) => new GloballyMappedDto2() { Value = obj.Value + 2 };
    }
}
