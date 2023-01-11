using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public partial class TestMapper
    {
        public partial int DirectInt(int value);

        public partial long ImplicitCastInt(int value);

        public partial int ExplicitCastInt(uint value);

        public partial int? CastIntNullable(int value);

        public partial Guid ParseableGuid(string id);

        public partial int ParseableInt(string value);

        public partial DateTime DirectDateTime(DateTime dateTime);

        public partial IEnumerable<TestObjectDto> MapAllDtos(IEnumerable<TestObject> objects);

        public TestObjectDto MapToDto(TestObject src)
        {
            var target = MapToDtoInternal(src);
            target.StringValue += "+after-map";
            return target;
        }

        // disable obsolete warning, as the obsolete attribute should still be tested.
#pragma warning disable CS0618
        [MapperIgnore(nameof(TestObjectDto.IgnoredStringValue))]
#pragma warning restore CS0618
        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
        [MapProperty(
            new[] { nameof(TestObject.UnflatteningIdValue) },
            new[] { nameof(TestObjectDto.Unflattening), nameof(TestObjectDto.Unflattening.IdValue) })]
        [MapProperty(
            nameof(TestObject.NullableUnflatteningIdValue),
            nameof(TestObjectDto.NullableUnflattening) + "." + nameof(TestObjectDto.NullableUnflattening.IdValue))]
        private partial TestObjectDto MapToDtoInternal(TestObject testObject);

        // disable obsolete warning, as the obsolete attribute should still be tested.
#pragma warning disable CS0618
        [MapperIgnore(nameof(TestObject.IgnoredStringValue))]
#pragma warning restore CS0618
        [MapperIgnoreTarget(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.IgnoredIntValue))]
        public partial TestObject MapFromDto(TestObjectDto dto);

        [MapEnum(EnumMappingStrategy.ByName)]
        public partial TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        public partial void UpdateDto(TestObject source, TestObjectDto target);

        private partial int PrivateDirectInt(int value);
    }
}
