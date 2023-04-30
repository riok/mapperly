using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    [UsesVerify]
    public class DeepCloningMapperTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(DeepCloningMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public Task RunMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = DeepCloningMapper.Copy(model);
            return Verifier.Verify(dto);
        }

        [Fact]
        public void RunIdMappingShouldWork()
        {
            var source = new IdObject { IdValue = 20 };
            var copy = DeepCloningMapper.Copy(source);
            source.Should().NotBeSameAs(copy);
            copy.IdValue.Should().Be(20);
        }
    }
}
