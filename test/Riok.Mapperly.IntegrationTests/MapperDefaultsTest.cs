using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class MapperDefaultsTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(EnumMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var enum2 = EnumMapper.Map(Enum1.Value1);
            enum2.Should().Be(Enum2.Value1);
        }
    }
}
