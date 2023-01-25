using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public static partial class StaticTestMapper
    {
        public static partial int DirectInt(int value);

        public static partial long ImplicitCastInt(int value);

        public static partial int ExplicitCastInt(uint value);

        public static partial int? CastIntNullable(int value);

        public static partial Guid ParseableGuid(string id);

        public static partial int ParseableInt(string value);

        public static partial DateTime DirectDateTime(DateTime dateTime);

        public static partial IEnumerable<TestObjectDto> MapAllDtos(IEnumerable<TestObject> objects);

        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        public static partial TestObjectDto MapToDtoExt(this TestObject src);

        public static TestObjectDto MapToDto(TestObject src)
        {
            var target = MapToDtoInternal(src);
            target.StringValue += "+after-map";
            return target;
        }

        // disable obsolete warning, as the obsolete attribute should still be tested.
#pragma warning disable CS0618
        [MapperIgnore(nameof(TestObjectDto.IgnoredStringValue))]
#pragma warning restore CS0618
        [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
        [MapProperty(
            new[] { nameof(TestObject.UnflatteningIdValue) },
            new[] { nameof(TestObjectDto.Unflattening), nameof(TestObjectDto.Unflattening.IdValue) })]
        [MapProperty(
            nameof(TestObject.NullableUnflatteningIdValue),
            nameof(TestObjectDto.NullableUnflattening) + "." + nameof(TestObjectDto.NullableUnflattening.IdValue))]
        [MapperIgnoreTarget(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.IgnoredIntValue))]
        private static partial TestObjectDto MapToDtoInternal(TestObject testObject);

        // disable obsolete warning, as the obsolete attribute should still be tested.
#pragma warning disable CS0618
        [MapperIgnore(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnore(nameof(TestObjectDto.DateTimeValueTargetDateOnly))]
        [MapperIgnore(nameof(TestObjectDto.DateTimeValueTargetTimeOnly))]
#pragma warning restore CS0618
        [MapperIgnoreTarget(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.IgnoredIntValue))]
        public static partial TestObject MapFromDto(TestObjectDto dto);

        [MapEnum(EnumMappingStrategy.ByName)]
        public static partial TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        public static partial void UpdateDto(TestObject source, TestObjectDto target);

        private static partial int PrivateDirectInt(int value);
    }
}
