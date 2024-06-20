using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByValue)]
    public static partial class StaticTestMapper
    {
        [UserMapping(Default = true)]
        public static partial int DirectInt(int value);

        public static partial int? DirectIntNullable(int? value);

        public static partial long ImplicitCastInt(int value);

        public static partial int ExplicitCastInt(uint value);

        public static partial int? CastIntNullable(int value);

        public static partial Guid ParseableGuid(string id);

        public static partial int ParseableInt(string value);

        public static partial DateTime DirectDateTime(DateTime dateTime);

        public static partial IEnumerable<TestObjectDto> MapAllDtos(IEnumerable<TestObject> objects);

        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredStringValue))]
        [MapperIgnoreObsoleteMembers]
        [MapperRequiredMapping(RequiredMappingStrategy.Target)]
        public static partial TestObjectDto MapToDtoExt(this TestObject src);

        [UserMapping(Default = true)]
        public static TestObjectDto MapToDto(TestObject src)
        {
            var target = MapToDtoInternal(src);
            target.StringValue += "+after-map";
            return target;
        }

        public static void MapExistingList(List<string> src, List<int> dst)
        {
            foreach (var item in src)
            {
                dst.Add(int.Parse(item));
            }
        }

        [MapProperty(nameof(TestObject.RenamedStringValue), nameof(TestObjectDto.RenamedStringValue2))]
        [MapProperty(
            new[] { nameof(TestObject.UnflatteningIdValue) },
            new[] { nameof(TestObjectDto.Unflattening), nameof(TestObjectDto.Unflattening.IdValue) }
        )]
        [MapProperty(
            nameof(TestObject.NullableUnflatteningIdValue),
            nameof(TestObjectDto.NullableUnflattening) + "." + nameof(TestObjectDto.NullableUnflattening.IdValue)
        )]
        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredStringValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreObsoleteMembers]
        private static partial TestObjectDto MapToDtoInternal(TestObject testObject);

        [MapperIgnoreTarget(nameof(TestObject.DateTimeValueTargetDateOnly))]
        [MapperIgnoreTarget(nameof(TestObject.DateTimeValueTargetTimeOnly))]
        [MapperIgnoreTarget(nameof(TestObject.IgnoredStringValue))]
        [MapperIgnoreTarget(nameof(TestObject.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectDto.SpanValue))]
        public static partial TestObject MapFromDto(TestObjectDto dto);

        [MapperIgnoreTarget(nameof(TestObjectDto.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObject.IgnoredStringValue))]
        public static partial void UpdateDto(TestObject source, TestObjectDto target);

        private static partial int PrivateDirectInt(int value);

#if NET7_0_OR_GREATER
        [MapDerivedType<string, int>]
        [MapDerivedType<int, string>]
#else
        [MapDerivedType(typeof(string), typeof(int))]
        [MapDerivedType(typeof(int), typeof(string))]
#endif
        public static partial object DerivedTypes(object source);

        public static partial object MapWithRuntimeTargetType(object source, Type targetType);

        public static partial object? MapNullableWithRuntimeTargetType(object? source, Type targetType);

        public static partial TTarget MapGeneric<TSource, TTarget>(TSource source);

#if NET7_0_OR_GREATER
        [MapDerivedType<ExistingObjectTypeA, ExistingObjectTypeA>]
        [MapDerivedType<ExistingObjectTypeB, ExistingObjectTypeB>]
#else
        [MapDerivedType(typeof(ExistingObjectTypeA), typeof(ExistingObjectTypeA))]
        [MapDerivedType(typeof(ExistingObjectTypeB), typeof(ExistingObjectTypeB))]
#endif
        public static partial void MapToDerivedExisting(ExistingObjectBase source, ExistingObjectBase target);

        [MapEnum(EnumMappingStrategy.ByName)]
        public static partial TestEnumDtoByName MapToEnumDtoByName(TestEnum v);

        [MapEnum(EnumMappingStrategy.ByName)]
        [MapEnumValue(TestEnumDtoAdditionalValue.Value40, TestEnum.Value30)]
        [MapEnumValue(TestEnumDtoAdditionalValue.Value50, TestEnum.Value30)]
        public static partial TestEnum MapToEnumByNameWithExplicit(TestEnumDtoAdditionalValue v);

        [MapEnum(EnumMappingStrategy.ByValue)]
        [MapEnumValue(TestEnumDtoAdditionalValue.Value40, TestEnum.Value30)]
        [MapEnumValue(TestEnumDtoAdditionalValue.Value50, TestEnum.Value30)]
        public static partial TestEnum MapToEnumByValueWithExplicit(TestEnumDtoAdditionalValue v);

        [MapEnum(EnumMappingStrategy.ByName)]
        [MapperIgnoreSourceValue(TestEnumDtoAdditionalValue.Value30)]
        [MapperIgnoreSourceValue(TestEnumDtoAdditionalValue.Value40)]
        [MapperIgnoreTargetValue(TestEnum.Value30)]
        public static partial TestEnum MapToEnumByNameWithIgnored(TestEnumDtoAdditionalValue v);

        [UserMapping(Default = true)]
        [MapEnum(EnumMappingStrategy.ByValueCheckDefined)]
        public static partial TestEnum MapToEnumByValueCheckDefined(TestEnumDtoByValue v);

        [MapEnum(EnumMappingStrategy.ByValueCheckDefined, FallbackValue = TestEnum.Value10)]
        public static partial TestEnum MapToEnumByValueCheckDefinedWithFallback(TestEnumDtoByValue v);

        [MapEnum(EnumMappingStrategy.ByValueCheckDefined)]
        public static partial TestFlagsEnum MapToFlagsEnumByValueCheckDefined(TestFlagsEnumDto v);

        [MapEnum(EnumMappingStrategy.ByName, FallbackValue = TestEnum.Value10)]
        public static partial TestEnum MapToEnumByNameWithFallback(TestEnumDtoByName v);

        [MapValue(nameof(ConstantValuesDto.CtorConstantValue), "fooBar")]
        [MapValue(nameof(ConstantValuesDto.ConstantValue), 1)]
        [MapValue(nameof(ConstantValuesDto.ConstantValueByMethod), Use = nameof(IntValueBuilder))]
        public static partial ConstantValuesDto MapConstantValues(ConstantValuesObject source);

        private static int IntValueBuilder() => 2;
    }
}
