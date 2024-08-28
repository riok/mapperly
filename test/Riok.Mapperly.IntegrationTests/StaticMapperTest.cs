using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Helpers;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using VerifyXunit;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class StaticMapperTest : BaseMapperTest
    {
        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task SnapshotGeneratedSource()
        {
            var path = GetGeneratedMapperFilePath(nameof(StaticTestMapper));
            return Verifier.VerifyFile(path);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task RunMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = StaticTestMapper.MapToDto(model);
            return Verifier.Verify(dto);
        }

        [Fact]
        [VersionedSnapshot(Versions.NET6_0)]
        public Task RunExtensionMappingShouldWork()
        {
            var model = NewTestObj();
            var dto = model.MapToDtoExt();
            return Verifier.Verify(dto);
        }

        [Fact]
        public void NestedListsShouldWork()
        {
            var l = new List<List<List<string>>>
            {
                new()
                {
                    new() { "1", "2", "3" },
                },
                new()
                {
                    new() { "4", "5" },
                    new() { "6" },
                },
            };
            var mapped = StaticTestMapper.MapNestedLists(l);

            mapped.Should().HaveCount(2);
            mapped[0].Should().HaveCount(1);
            mapped[0][0].Should().HaveCount(3);
            mapped[1].Should().HaveCount(2);
            mapped[1][0].Should().HaveCount(2);
            mapped[1][1].Should().HaveCount(1);
            mapped.SelectMany(x => x).SelectMany(x => x).Should().BeEquivalentTo(Enumerable.Range(1, 6), x => x.WithStrictOrdering());
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

        [Fact]
        public Task ConstantValuesShouldWork()
        {
            var obj = new ConstantValuesObject { CtorMappedValue = 10, MappedValue = 11 };
            var dto = StaticTestMapper.MapConstantValues(obj);
            return Verifier.Verify(dto);
        }

        [Fact]
        public void RunMappingIdTargetExtShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            model.MapIdTargetExt(new IdObjectDto { IdValue = 20 });
            model.IdValue.Should().Be(20);
        }

        [Fact]
        public void RunMappingIdTargetFirstShouldWork()
        {
            var model = new IdObject { IdValue = 10 };
            StaticTestMapper.MapIdTargetFirst(model, new IdObjectDto { IdValue = 20 });
            model.IdValue.Should().Be(20);
        }

        [Fact]
        public void MapWithAdditionalParameterShouldWork()
        {
            var dto = StaticTestMapper.MapWithAdditionalParameter(new IdObject { IdValue = 1 }, 2);
            dto.IdValue.Should().Be(1);
            dto.ValueFromParameter.Should().Be(2);
        }
    }
}
