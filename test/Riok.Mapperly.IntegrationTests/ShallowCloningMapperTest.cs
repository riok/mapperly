using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class ShallowCloningMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET8_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(ShallowCloningMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET8_0 | Versions.NET9_0)]
        public Task RunMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = ShallowCloningMapper.Copy(model);
            return Verifier.Verify(dto);
        }

        [Fact]
        public void RunIdMappingShouldWork()
        {
            var source = new IdObject { IdValue = 20 };
            var copy = ShallowCloningMapper.Copy(source);
            source.ShouldNotBeSameAs(copy);
            copy.IdValue.ShouldBe(20);
        }

        [Fact]
        public void RunMappingWithMapperAvoidReturningSourceReference()
        {
            var source = new TestObject(255, -1, 7) { RequiredValue = 999 };
            var copy = ShallowCloningMapper.Copy(source);
            source.ShouldNotBeSameAs(copy);
            copy.RequiredValue.ShouldBe(999);
        }

        [Fact]
        public void RunMappingWithMapperAvoidCloningChildObjects()
        {
            var nested = new TestObjectNested() { IntValue = int.MaxValue };

            var idObject = new IdObject() { IdValue = 7 };

            var source = new TestObject(255, -1, 7)
            {
                RequiredValue = 999,
                NestedNullable = nested,
                NestedNullableTargetNotNullable = nested,
                Flattening = idObject,
            };
            var copy = ShallowCloningMapper.Copy(source);
            source.ShouldNotBeSameAs(copy);
            copy.RequiredValue.ShouldBe(999);

            // check the references are exactly the same
            copy.Flattening.ShouldBeSameAs(idObject);
            copy.NestedNullable.ShouldBeSameAs(nested);
            copy.NestedNullableTargetNotNullable.ShouldBeSameAs(nested);
        }
    }
}
