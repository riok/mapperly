using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

#if NET8_0_OR_GREATER
using AliasedTupleSource = (int X, int Y);
using AliasedTupleTarget = (string X, string Y);
#endif

namespace Riok.Mapperly.IntegrationTests.Mapper
{
#if NET8_0_OR_GREATER
    [Mapper(IncludedMembers = MemberVisibility.All)]
#else
    [Mapper]
#endif
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

        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredStringValue))]
        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
        [MapProperty(
            new[] { nameof(TestObject.UnflatteningIdValue) },
            new[] { nameof(TestObjectDto.Unflattening), nameof(TestObjectDto.Unflattening.IdValue) }
        )]
        [MapProperty(
            nameof(TestObject.NullableUnflatteningIdValue),
            nameof(TestObjectDto.NullableUnflattening) + "." + nameof(TestObjectDto.NullableUnflattening.IdValue)
        )]
        [MapperIgnoreObsoleteMembers]
        private partial TestObjectDto MapToDtoInternal(TestObject testObject);

        [MapperIgnoreTarget(nameof(TestObject.DateTimeValueTargetDateOnly))]
        [MapperIgnoreTarget(nameof(TestObject.DateTimeValueTargetTimeOnly))]
        [MapperIgnoreTarget(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnoreTarget(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.IgnoredIntValue))]
        public partial TestObject MapFromDto(TestObjectDto dto);

        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredStringValue))]
        public partial void UpdateDto(TestObject source, TestObjectDto target);

        [MapEnum(EnumMappingStrategy.ByName)]
        public partial TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

        private partial int PrivateDirectInt(int value);

#if NET8_0_OR_GREATER
        public partial AliasedTupleTarget MapAliasedTuple(AliasedTupleSource source);
#endif
    }
}
