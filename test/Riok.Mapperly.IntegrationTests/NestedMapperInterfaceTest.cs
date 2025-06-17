using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class NestedMapperInterfaceTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath($"{nameof(INestedTestMapper)}.{nameof(NestedTestMapper.TestNesting.NestedMapper)}");
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var v = INestedTestMapper.NestedMapper.ToInt(10.25m);
            v.ShouldBe(10);
        }
    }
}
