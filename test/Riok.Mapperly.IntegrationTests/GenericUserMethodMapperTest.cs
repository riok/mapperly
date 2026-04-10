using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class GenericUserMethodMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(GenericUserMethodMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var mapper = new GenericUserMethodMapper();
            var model = new Document("Design Doc", new User("Alice"), Optional.Of(new User("Bob")));
            var dto = mapper.MapDocument(model);

            dto.Title.ShouldBe("Design Doc");
            dto.CreatedBy.Name.ShouldBe("Alice");
            dto.ModifiedBy.HasValue.ShouldBeTrue();
            dto.ModifiedBy.Value.Name.ShouldBe("Bob");
        }

        [Fact]
        public void RunMappingWithEmptyOptionalShouldWork()
        {
            var mapper = new GenericUserMethodMapper();
            var model = new Document("Draft", new User("Charlie"), Optional.Empty<User>());
            var dto = mapper.MapDocument(model);

            dto.Title.ShouldBe("Draft");
            dto.CreatedBy.Name.ShouldBe("Charlie");
            dto.ModifiedBy.HasValue.ShouldBeFalse();
        }
    }
}
