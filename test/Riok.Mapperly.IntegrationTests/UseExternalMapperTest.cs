using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class UseExternalMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(UseExternalMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.Map(model);
            dto.IdValue.ShouldBe(100);
        }

        [Fact]
        public void RunMapExternalShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.MapExternal(model);
            dto.IdValue.ShouldBe(11);
        }

        [Fact]
        public void RunMapFromSourceExternalShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.MapFromSourceExternal(model);
            dto.IdValue.ShouldBe(12);
        }

        [Fact]
        public void RunConstantMapExternalShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.ConstantMapExternal(model);
            dto.IdValue.ShouldBe(13);
        }

        [Fact]
        public void RunMapExternalStringShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.MapExternalString(model);
            dto.IdValue.ShouldBe(11);
        }

        [Fact]
        public void RunMapFromSourceExternalStringShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.MapFromSourceExternalString(model);
            dto.IdValue.ShouldBe(12);
        }

        [Fact]
        public void RunConstantMapExternalStringShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = UseExternalMapper.ConstantMapExternalString(model);
            dto.IdValue.ShouldBe(13);
        }
    }
}
