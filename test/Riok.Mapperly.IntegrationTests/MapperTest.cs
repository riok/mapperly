using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    [UsesVerify]
    [VersionedSnapshot(Versions.NET6_0)]
    public class MapperTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(TestMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public Task RunMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = new TestMapper().MapToDto(model);
            return Verifier.Verify(dto);
        }
    }
}
