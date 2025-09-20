using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class UseExternalInstanceMapperTest : BaseMapperTest
    {
        private readonly UseExternalInstanceMapper _mapper = new();

        [Fact]
        [VersionedSnapshot(Versions.NET6_0 | Versions.NET8_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(UseExternalInstanceMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public void RunMappingShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = _mapper.Map(model);
            dto.IdValue.ShouldBe(100);
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void RunMapExternalShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = _mapper.MapExternal(model);
            dto.IdValue.ShouldBe(11);
        }

        [Fact]
        public void RunMapFromSourceExternalShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = _mapper.MapFromSourceExternal(model);
            dto.IdValue.ShouldBe(12);
        }

        [Fact]
        public void RunConstantMapExternalShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            var dto = _mapper.ConstantMapExternal(model);
            dto.IdValue.ShouldBe(13);
        }
#endif
    }
}
