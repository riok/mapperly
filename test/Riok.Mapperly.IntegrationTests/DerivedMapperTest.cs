using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class DerivedMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSourceBaseMapper()
        {
            var path = GetGeneratedMapperFilePath(nameof(BaseMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSourceDerivedMapper()
        {
            var path = GetGeneratedMapperFilePath(nameof(DerivedMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSourceDerivedMapper2()
        {
            var path = GetGeneratedMapperFilePath(nameof(DerivedMapper2));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            new BaseMapper().IntToLong(10).ShouldBe(10L);
            new BaseMapper().IntToShort(10).ShouldBe((short)10);
            new DerivedMapper().IntToLong(10).ShouldBe(10L);
            new DerivedMapper().IntToShort(10).ShouldBe((short)10);
            new DerivedMapper2().IntToLong(10).ShouldBe(10L);
            new DerivedMapper2().IntToShort(10).ShouldBe((short)10);
        }
    }
}
