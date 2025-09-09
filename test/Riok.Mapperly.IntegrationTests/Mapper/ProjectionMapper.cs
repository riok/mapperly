using System.Collections.Generic;
using System.Linq;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByValue, AutoUserMappings = false)]
    public static partial class ProjectionMapper
    {
        public static partial IQueryable<TestObjectDtoProjection> ProjectToDto(this IQueryable<TestObjectProjection> q);

        public static partial IQueryable<TestObjectDtoProjectionBaseType> ProjectToDto(this IQueryable<TestObjectProjectionBaseType> q);

        public static partial IQueryable<TestObjectDtoProjectionWithParameters> ProjectToDto(
            this IQueryable<TestObjectProjectionBaseType> q,
            int valueFromParameter
        );

        [MapperIgnoreTarget(nameof(TestObjectDtoProjection.IgnoredStringValue))]
        [MapperIgnoreTarget(nameof(TestObjectDtoProjection.IgnoredIntValue))]
        [MapperIgnoreSource(nameof(TestObjectProjection.IgnoredStringValue))]
        [MapProperty(nameof(TestObjectProjection.RenamedStringValue), nameof(TestObjectDtoProjection.RenamedStringValue2))]
        [MapProperty(
            nameof(TestObjectProjection.ManuallyMappedModified),
            nameof(TestObjectDtoProjection.ManuallyMappedModified),
            Use = nameof(ModifyInt)
        )]
        [MapProperty(
            nameof(TestObjectProjection.ManuallyMappedNullableToNonNullable),
            nameof(TestObjectDtoProjection.ManuallyMappedNullableToNonNullable),
            Use = nameof(MapNullableToNonNullableInt)
        )]
        [MapperIgnoreObsoleteMembers]
        private static partial TestObjectDtoProjection ProjectToDto(this TestObjectProjection testObject);

        [UserMapping]
        private static TestObjectDtoManuallyMappedProjection? MapManual(string str) => new(100) { StringValue = str };

        [UserMapping]
        private static TestEnum MapManual(TestObjectProjectionEnumValue source) => source.Value;

        [MapProperty(
            nameof(TestObjectProjectionBaseType.BaseValue),
            nameof(TestObjectDtoProjectionWithParameters.ValueFromParameter2),
            Use = nameof(MultiplyValue)
        )]
        private static partial TestObjectDtoProjectionWithParameters MapWithParameters(
            TestObjectProjectionBaseType source,
            int valueFromParameter
        );

        private static partial IQueryable<TestObjectDtoProjectionWithParameters> ProjectWithParameters(
            this IQueryable<TestObjectProjectionBaseType> source,
            int valueFromParameter
        );

        [MapDerivedType(typeof(TestObjectProjectionTypeA), typeof(TestObjectDtoProjectionTypeA))]
        [MapDerivedType(typeof(TestObjectProjectionTypeB), typeof(TestObjectDtoProjectionTypeB))]
        private static partial TestObjectDtoProjectionBaseType MapDerived(TestObjectProjectionBaseType source);

        [UserMapping]
        private static ICollection<IntegerValue> OrderIntegerValues(ICollection<IntegerValue> values) =>
            values.OrderBy(x => x.Value).ToList();

        [UserMapping]
        private static ICollection<LongValueDto> OrderAndMapLongValues(ICollection<LongValue> values) =>
            values.OrderBy(x => x.Value).Select(x => MapLongValue(x)).ToList();

        [MapperIgnoreSource(nameof(LongValue.Id))]
        private static partial LongValueDto MapLongValue(LongValue value);

        private static int ModifyInt(int v) => v + 10;

        private static int MapNullableToNonNullableInt(int? v) => v ?? -1;

        private static int MultiplyValue(int baseValue, int valueFromParameter) => baseValue * valueFromParameter;
    }
}
