using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    [UsesVerify]
    public class DerivedMapperTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSourceBaseMapper()
        {
            var path = GetGeneratedMapperFilePath(nameof(BaseMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public Task SnapshotGeneratedSourceDerivedMapper()
        {
            var path = GetGeneratedMapperFilePath(nameof(DerivedMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public Task SnapshotGeneratedSourceDerivedMapper2()
        {
            var path = GetGeneratedMapperFilePath(nameof(DerivedMapper2));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            new BaseMapper().IntToLong(10).Should().Be(10L);
            new BaseMapper().IntToShort(10).Should().Be(10);
            new DerivedMapper().IntToLong(10).Should().Be(10L);
            new DerivedMapper().IntToShort(10).Should().Be(10);
            new DerivedMapper2().IntToLong(10).Should().Be(10L);
            new DerivedMapper2().IntToShort(10).Should().Be(10);
        }
    }
}
