using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
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
            dto.Value.Should().Be(1);
            dto.Parent.Should().NotBeNull();
            dto.Parent!.Value.Should().Be(2);
            dto.Parent.Parent.Should().Be(dto);
        }

        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(CircularReferenceMapper));
            return Verifier.VerifyFile(path);
        }
    }
}
