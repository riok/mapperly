using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    [UseStaticMapper(typeof(MyOtherMapper))]
    public static partial class UseExternalMapper
    {
        public static partial IdObjectDto Map(IdObject source);

        [MapProperty(nameof(IdObject.IdValue), nameof(IdObjectDto.IdValue), Use = nameof(@ExternalMapperMethods.MapStatic))]
        public static partial IdObjectDto MapExternal(IdObject source);

        [MapPropertyFromSource(nameof(IdObjectDto.IdValue), Use = nameof(@ExternalMapperMethods.ComputeSumStatic))]
        public static partial IdObjectDto MapFromSourceExternal(IdObject source);

        [MapValue(nameof(IdObjectDto.IdValue), Use = nameof(@ExternalMapperMethods.IntValueStatic))]
        public static partial IdObjectDto ConstantMapExternal(IdObject source);

        [MapProperty(
            nameof(IdObject.IdValue),
            nameof(IdObjectDto.IdValue),
            Use = "Riok.Mapperly.IntegrationTests.Mapper.ExternalMapperMethods.MapStatic"
        )]
        public static partial IdObjectDto MapExternalString(IdObject source);

        [MapPropertyFromSource(
            nameof(IdObjectDto.IdValue),
            Use = "Riok.Mapperly.IntegrationTests.Mapper.ExternalMapperMethods.ComputeSumStatic"
        )]
        public static partial IdObjectDto MapFromSourceExternalString(IdObject source);

        [MapValue(nameof(IdObjectDto.IdValue), Use = "Riok.Mapperly.IntegrationTests.Mapper.ExternalMapperMethods.IntValueStatic")]
        public static partial IdObjectDto ConstantMapExternalString(IdObject source);

        public static partial ExternalItemsDto ToDto(ExternalItemsModel obj);

        public static class MyOtherMapper
        {
            public static int MapInt(int source) => source * 10;
        }
    }
}
