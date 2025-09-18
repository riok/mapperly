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
    public class NamedMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(NamedMappings));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var model = NewNamedMappingObject();
            var dto = NamedMappings.MapWithNamedMappings(model);
            dto.FromMapPropertyUse.ShouldBe("Test-modified");
            dto.FromMapValue.ShouldBe("fooBar");
            dto.FromMapPropertyFromSource.ShouldBe("Test-from-source");
        }

        [Fact]
        public void RunMappingWithIncludeShouldWork()
        {
            var model = NewNamedMappingObject();
            var dto = new NamedMappingValuesDto();
            NamedMappings.UpdateDto(model, dto);
            dto.FromMapPropertyUse.ShouldBe("Test-modified");
            dto.FromMapValue.ShouldBe("fooBar");
            dto.FromMapPropertyFromSource.ShouldBe("Test-from-source");
        }

        private static NamedMappingObject NewNamedMappingObject() => new() { SourceValue = "Test" };
    }
}
