using System.Linq;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public static partial class AdditionalParameterInliningMapper
    {
        private static partial AdditionalParametersDto MapToDto(IdObject source, int valueFromParameter);

        public static partial IQueryable<AdditionalParametersDto> ProjectWithAdditionalParameter(
            this IQueryable<IdObject> q,
            int valueFromParameter
        );
    }
}
