using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class MappingTargetOriginalValueMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(MappingTargetOriginalValueMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void MapToDtoShouldUseOptionalValueWhenPresent()
        {
            var mapper = new MappingTargetOriginalValueMapper();
            var source = new OptionalObject { Name = Optional.Of("Alice") };

            var dto = mapper.MapToDto(source);

            dto.Name.ShouldBe("Alice");
        }

        [Fact]
        public void MapToDtoShouldUseDefaultWhenOptionalEmpty()
        {
            var mapper = new MappingTargetOriginalValueMapper();
            var source = new OptionalObject();

            var dto = mapper.MapToDto(source);

            dto.Name.ShouldBeNull();
        }

        [Fact]
        public void UpdateDtoShouldPreserveExistingValueWhenOptionalEmpty()
        {
            var mapper = new MappingTargetOriginalValueMapper();
            var dto = new OptionalDto { Name = "Bob" };
            var source = new OptionalObject();

            mapper.UpdateDto(dto, source);

            dto.Name.ShouldBe("Bob");
        }

        [Fact]
        public void UpdateDtoShouldOverwriteExistingValueWhenOptionalHasValue()
        {
            var mapper = new MappingTargetOriginalValueMapper();
            var dto = new OptionalDto { Name = "Bob" };
            var source = new OptionalObject { Name = Optional.Of("Alice") };

            mapper.UpdateDto(dto, source);

            dto.Name.ShouldBe("Alice");
        }
    }
}
