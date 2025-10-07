using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public partial class UseExternalInstanceMapper
    {
        [UseMapper]
        private readonly MyOtherMapper _myOtherMapper = new();

        private readonly ExternalMapperMethods _externalMapper = new();

        public partial IdObjectDto Map(IdObject source);

#if NET8_0_OR_GREATER
        [MapProperty(nameof(IdObject.IdValue), nameof(IdObjectDto.IdValue), Use = nameof(@_externalMapper.MapInstance))]
        public partial IdObjectDto MapExternal(IdObject source);

        [MapValue(nameof(IdObjectDto.IdValue), Use = nameof(@_externalMapper.IntValueInstance))]
        public partial IdObjectDto ConstantMapExternal(IdObject source);

        [MapPropertyFromSource(nameof(IdObjectDto.IdValue), Use = nameof(@_externalMapper.ComputeSumInstance))]
        public partial IdObjectDto MapFromSourceExternal(IdObject source);
#endif

        [MapProperty(nameof(IdObject.IdValue), nameof(IdObjectDto.IdValue), Use = "_externalMapper.MapInstance")]
        public partial IdObjectDto MapExternalWithString(IdObject source);

        [MapValue(nameof(IdObjectDto.IdValue), Use = "_externalMapper.IntValueInstance")]
        public partial IdObjectDto ConstantMapExternalWithString(IdObject source);

        [MapPropertyFromSource(nameof(IdObjectDto.IdValue), Use = "_externalMapper.ComputeSumInstance")]
        public partial IdObjectDto MapFromSourceExternalWithString(IdObject source);

        public class MyOtherMapper
        {
            public int MapInt(int source) => source * 10;
        }
    }
}
