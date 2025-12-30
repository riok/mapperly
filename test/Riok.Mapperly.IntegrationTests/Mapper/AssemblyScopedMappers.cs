using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;

[assembly: UseStaticMapper(typeof(AssemblyScopedMappers))]

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static class AssemblyScopedMappers
    {
        public static AssemblyScopedDto ToDto(AssemblyScopedModel obj) => new AssemblyScopedDto() { Value = obj.Value + 1 };
    }
}
