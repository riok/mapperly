using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class ReverseMappingTest : BaseMapperTest
    {
        [Fact]
        public void ForwardMappingShouldWork()
        {
            var product = new ReverseProduct { Name = "Widget", Price = 9.99m };
            var dto = ReverseMappingMapper.ToDto(product);
            dto.ProductName.ShouldBe("Widget");
            dto.PriceInEuro.ShouldBe(9.99m);
        }

        [Fact]
        public void ReverseMappingShouldWork()
        {
            var dto = new ReverseProductDto { ProductName = "Widget", PriceInEuro = 9.99m };
            var product = ReverseMappingMapper.ToProduct(dto);
            product.Name.ShouldBe("Widget");
            product.Price.ShouldBe(9.99m);
        }
    }
}
