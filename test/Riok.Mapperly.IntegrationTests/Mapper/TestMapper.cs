using System;
using System.Collections.Generic;
using System.Globalization;
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
    [Mapper(
        IncludedMembers = MemberVisibility.All,
        IncludedConstructors = MemberVisibility.All,
        EnumMappingStrategy = EnumMappingStrategy.ByValue
    )]
#else
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByValue)]
#endif
    public partial class TestMapper
    {
        [FormatProvider(Default = true)]
        private readonly CultureInfo _formatDeCh = (CultureInfo)CultureInfo.GetCultureInfo("de-CH").Clone();

        [FormatProvider]
        private readonly CultureInfo _formatEnUs = (CultureInfo)CultureInfo.GetCultureInfo("en-US").Clone();

        public TestMapper()
        {
            // these seem to vary depending on the OS
            // set them to a fixed value.
            _formatDeCh.NumberFormat.CurrencyPositivePattern = 2;
            _formatEnUs.NumberFormat.CurrencyPositivePattern = 2;
        }

        [UserMapping(Default = true)]
        public partial int DirectInt(int value);

        public partial long ImplicitCastInt(int value);

        public partial int ExplicitCastInt(uint value);

        public partial int? CastIntNullable(int value);

        public partial Guid ParseableGuid(string id);

        public partial int ParseableInt(string value);

        public partial DateTime DirectDateTime(DateTime dateTime);

#if NET5_0_OR_GREATER
        public partial byte[] ConvertWithInstanceMethod(Guid id);
#endif

        public partial IEnumerable<TestObjectDto> MapAllDtos(IEnumerable<TestObject> objects);

        [UserMapping(Default = true)]
        public TestObjectDto MapToDto(TestObject src)
        {
            var target = MapToDtoInternal(src);
            target.StringValue += "+after-map";
            return target;
        }

        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredStringValue))]
        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapProperty(nameof(TestObject.IntValue), nameof(TestObjectDto.FormattedIntValue), StringFormat = "C")]
        [MapProperty(
            nameof(TestObject.DateTimeValue),
            nameof(TestObjectDto.FormattedDateValue),
            StringFormat = "D",
            FormatProvider = nameof(_formatEnUs)
        )]
        [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
        [MapProperty(
            nameof(TestObject.UnflatteningIdValue),
            new[] { nameof(TestObjectDto.Unflattening), nameof(TestObjectDto.Unflattening.IdValue) }
        )]
        [MapProperty(
            nameof(TestObject.NullableUnflatteningIdValue),
            nameof(TestObjectDto.NullableUnflattening) + "." + nameof(TestObjectDto.NullableUnflattening.IdValue)
        )]
        [MapPropertyFromSource(nameof(TestObjectDto.Sum), Use = nameof(ComputeSum))]
        [MapNestedProperties(nameof(TestObject.NestedMember))]
        [MapperIgnoreObsoleteMembers]
        private partial TestObjectDto MapToDtoInternal(TestObject testObject);

        [MapperIgnoreTarget(nameof(TestObject.DateTimeValueTargetDateOnly))]
        [MapperIgnoreTarget(nameof(TestObject.DateTimeValueTargetTimeOnly))]
        [MapperIgnoreTarget(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnoreTarget(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.SpanValue))]
        [MapProperty(nameof(TestObjectDto.FormattedIntValue), nameof(TestObject.IntValue), FormatProvider = nameof(_formatEnUs))]
        public partial TestObject MapFromDto(TestObjectDto dto);

        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredStringValue))]
        public partial void UpdateDto(TestObject source, TestObjectDto target);

        [MapEnum(EnumMappingStrategy.ByName)]
        public partial TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

        private partial int PrivateDirectInt(int value);

        [UserMapping(Default = false)]
        private int ComputeSum(TestObject testObject) => testObject.SumComponent1 + testObject.SumComponent2;

        [IncludeMappingConfiguration(nameof(MapToDtoInternal))]
        private partial TestObjectDto MapToDtoInternalInclude(TestObject testObject);

#if NET8_0_OR_GREATER
        public partial PrivateCtorDto MapPrivateDto(PrivateCtorObject testObject);

        public partial AliasedTupleTarget MapAliasedTuple(AliasedTupleSource source);
#endif
    }
}
