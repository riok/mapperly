using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class DeepCloningMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(DeepCloningMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET8_0 | Versions.NET9_0)]
        public Task RunMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = DeepCloningMapper.Copy(model);
            return Verifier.Verify(dto);
        }

        [Fact]
        public void RunIdMappingShouldWork()
        {
            var source = new IdObject { IdValue = 20 };
            var copy = DeepCloningMapper.Copy(source);
            source.ShouldNotBeSameAs(copy);
            copy.IdValue.ShouldBe(20);
        }

        [Fact]
        public void RunMappingWithMapperAvoidReturningSourceReference()
        {
            var source = new TestObject(255, -1, 7) { RequiredValue = 999 };
            var copy = AvoidReturningSourceReferenceMapper.Copy(source);
            source.ShouldNotBeSameAs(copy);
            copy.RequiredValue.ShouldBe(999);
        }
    }
}
