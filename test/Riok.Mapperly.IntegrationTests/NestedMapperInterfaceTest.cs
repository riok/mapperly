using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class NestedMapperInterfaceTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath($"{nameof(INestedTestMapper)}.{nameof(NestedTestMapper.TestNesting.NestedMapper)}");
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var v = INestedTestMapper.NestedMapper.ToInt(10.25m);
            v.Should().Be(10);
        }
    }
}
