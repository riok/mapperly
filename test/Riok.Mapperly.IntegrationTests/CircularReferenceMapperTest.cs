using System.Threading.Tasks;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class CircularReferenceMapperTest : BaseMapperTest
    {
        [Fact]
        public void ShouldMapCircularReference()
        {
            var obj = new CircularReferenceObject
            {
                Value = 1,
                Parent = new() { Value = 2 },
            };
            obj.Parent.Parent = obj;

            var dto = CircularReferenceMapper.ToDto(obj);
            dto.Value.ShouldBe(1);
            dto.Parent.ShouldNotBeNull();
            dto.Parent!.Value.ShouldBe(2);
            dto.Parent.Parent.ShouldBe(dto);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(CircularReferenceMapper));
            return Verifier.VerifyFile(path);
        }
    }
}
