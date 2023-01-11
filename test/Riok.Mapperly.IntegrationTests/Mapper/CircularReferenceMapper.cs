using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(UseReferenceHandling = true)]
    public static partial class CircularReferenceMapper
    {
        public static partial CircularReferenceDto ToDto(CircularReferenceObject obj);
    }
}
