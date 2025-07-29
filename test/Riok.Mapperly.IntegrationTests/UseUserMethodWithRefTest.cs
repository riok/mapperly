using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class UseUserMethodWithRefTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(UseUserMethodWithRef));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunArrayMappingWithRef()
        {
            var modelTarget = new ArrayObject { IntArray = new[] { 10, 12 } };
            var modelSrc = new ArrayObject { IntArray = new[] { 11, 13 } };
            UseUserMethodWithRef.Merge(modelTarget, modelSrc);
            modelTarget.IntArray.ShouldBe(new[] { 10, 12, 11, 13 });
        }
    }
}
