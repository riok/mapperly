using System.Linq;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
    public static partial class NoExpressionInliningEnumMapper
    {
        public static partial NoExpressionInliningProjectionStatusDto MapStatus(NoExpressionInliningProjectionStatus status);
    }

    [Mapper]
    [UseStaticMapper(typeof(NoExpressionInliningEnumMapper), NoExpressionInlining = true)]
    public static partial class NoExpressionInliningProjectionMapper
    {
        public static partial IQueryable<NoExpressionInliningProjectionDto> ProjectToDto(
            this IQueryable<NoExpressionInliningProjectionSource> source
        );
    }
}
