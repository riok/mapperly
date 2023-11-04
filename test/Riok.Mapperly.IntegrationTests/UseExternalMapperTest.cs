using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    [UsesVerify]
    public class UseExternalMapperTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(UseExternalMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.Map(model);
            dto.IdValue.Should().Be(100);
        }
    }
}
