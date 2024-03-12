using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class NestedMapperTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(
                $"{nameof(NestedTestMapper)}.{nameof(NestedTestMapper.TestNesting)}.{nameof(NestedTestMapper.TestNesting.NestedMapper)}"
            );
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var v = NestedTestMapper.TestNesting.NestedMapper.ToInt(10.25m);
            v.Should().Be(10);
        }
    }
}
