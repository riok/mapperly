using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class NestedMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
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
            v.ShouldBe(10);
        }
    }
}
