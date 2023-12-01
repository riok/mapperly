using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using VerifyXunit;
using Xunit;
#if NET8_0_OR_GREATER
using FluentAssertions;
#endif

namespace Riok.Mapperly.IntegrationTests
{
    [UsesVerify]
    public class MapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET6_0 | Versions.NET8_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(TestMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET6_0 | Versions.NET8_0)]
        public Task RunMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = new TestMapper().MapToDto(model);
            return Verifier.Verify(dto);
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void RunMappingAliasedTuple()
        {
            var source = (10, 20);
            var target = new TestMapper().MapAliasedTuple(source);
            target.X.Should().Be("10");
            target.Y.Should().Be("20");
        }
#endif
    }
}
