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

        public static class MyOtherMapper
        {
            public static int MapInt(int source) => source * 10;
        }
    }
}
