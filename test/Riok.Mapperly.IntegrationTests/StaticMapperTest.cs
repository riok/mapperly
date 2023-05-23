using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    [UsesVerify]
    public class StaticMapperTest : BaseMapperTest
    {
        [Fact]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(StaticTestMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        public Task RunMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = StaticTestMapper.MapToDto(model);
            return Verifier.Verify(dto);
        }

        [Fact]
        public Task RunExtensionMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = model.MapToDtoExt();
            return Verifier.Verify(dto);
        }

        [Fact]
        public void DerivedTypesShouldWork()
        {
            StaticTestMapper.DerivedTypes("10").Should().Be(10);
            StaticTestMapper.DerivedTypes(10).Should().Be("10");
        }

        [Fact]
        public void RuntimeTargetTypeShouldWork()
        {
            StaticTestMapper.MapWithRuntimeTargetType("10", typeof(int)).Should().Be(10);
        }

        [Fact]
        public void NullableRuntimeTargetTypeWithNullShouldReturnNull()
        {
            StaticTestMapper.MapNullableWithRuntimeTargetType(null, typeof(int?)).Should().BeNull();
        }

        [Fact]
        public void GenericShouldWork()
        {
            var obj = NewTestObj();
            var dto = StaticTestMapper.MapGeneric<TestObject, TestObjectDto>(obj);
            dto.IntValue.Should().Be(obj.IntValue);
        }
    }
}
