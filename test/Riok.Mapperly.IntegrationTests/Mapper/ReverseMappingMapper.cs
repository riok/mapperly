using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public static partial class ReverseMappingMapper
    {
        [MapProperty(nameof(ReverseProduct.Name), nameof(ReverseProductDto.ProductName))]
        [MapProperty(nameof(ReverseProduct.Price), nameof(ReverseProductDto.PriceInEuro))]
        public static partial ReverseProductDto ToDto(ReverseProduct source);

        [IncludeMappingConfiguration(nameof(ToDto), Reverse = true)]
        public static partial ReverseProduct ToProduct(ReverseProductDto source);
    }
}
