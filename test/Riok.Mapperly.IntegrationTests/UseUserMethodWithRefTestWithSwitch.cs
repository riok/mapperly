using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class UseUserMethodWithRefTestWithSwitch : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(UseUserMethodWithRefWithSwitch));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunArrayMappingWithRefWithSwitch()
        {
            var modelTarget = new TestObjectProjectionTypeB { BaseValue = 5 };
            var modelSrc = new TestObjectProjectionTypeB { BaseValue = 6 };
            UseUserMethodWithRefWithSwitch.Merge(modelTarget, modelSrc);
            modelTarget.BaseValue.ShouldBe(11);
        }
    }
}
