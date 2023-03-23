#nullable enable
namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static partial class ProjectionMapper
    {
        public static partial System.Linq.IQueryable<Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection> ProjectToDto(this System.Linq.IQueryable<Riok.Mapperly.IntegrationTests.Models.TestObjectProjection> q)
        {
#nullable disable
            return System.Linq.Queryable.Select(q, x => new Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection(x.CtorValue)
            {IntValue = x.IntValue, IntInitOnlyValue = x.IntInitOnlyValue, RequiredValue = x.RequiredValue, StringValue = x.StringValue, RenamedStringValue2 = x.RenamedStringValue, FlatteningIdValue = x.Flattening.IdValue, NullableFlatteningIdValue = x.NullableFlattening != null ? x.NullableFlattening.IdValue : default, NestedNullableIntValue = x.NestedNullable != null ? x.NestedNullable.IntValue : default, NestedNullable = x.NestedNullable != null ? new Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto()
            {IntValue = x.NestedNullable.IntValue} : default, NestedNullableTargetNotNullable = x.NestedNullableTargetNotNullable != null ? new Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto()
            {IntValue = x.NestedNullableTargetNotNullable.IntValue} : new Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto(), StringNullableTargetNotNullable = x.StringNullableTargetNotNullable ?? "", SourceTargetSameObjectType = x.SourceTargetSameObjectType, NullableReadOnlyObjectCollection = x.NullableReadOnlyObjectCollection != null ? System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(x.NullableReadOnlyObjectCollection, x1 => new Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto()
            {IntValue = x1.IntValue})) : default, EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)x.EnumValue, EnumName = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName)x.EnumName, EnumRawValue = (byte)x.EnumRawValue, EnumStringValue = (string)x.EnumStringValue.ToString(), EnumReverseStringValue = System.Enum.Parse<Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue>(x.EnumReverseStringValue, false), SubObject = x.SubObject != null ? new Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto()
            {SubIntValue = x.SubObject.SubIntValue, BaseIntValue = x.SubObject.BaseIntValue} : default, DateTimeValueTargetDateOnly = System.DateOnly.FromDateTime(x.DateTimeValueTargetDateOnly), DateTimeValueTargetTimeOnly = System.TimeOnly.FromDateTime(x.DateTimeValueTargetTimeOnly)});
#nullable enable
        }

        private static partial Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection ProjectToDto(this Riok.Mapperly.IntegrationTests.Models.TestObjectProjection testObject)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection(testObject.CtorValue)
            {IntInitOnlyValue = testObject.IntInitOnlyValue, RequiredValue = testObject.RequiredValue};
            if (testObject.NestedNullable != null)
            {
                target.NestedNullableIntValue = testObject.NestedNullable.IntValue;
                target.NestedNullable = MapToTestObjectNestedDto(testObject.NestedNullable);
            }

            if (testObject.NestedNullableTargetNotNullable != null)
            {
                target.NestedNullableTargetNotNullable = MapToTestObjectNestedDto(testObject.NestedNullableTargetNotNullable);
            }

            if (testObject.StringNullableTargetNotNullable != null)
            {
                target.StringNullableTargetNotNullable = testObject.StringNullableTargetNotNullable;
            }

            if (testObject.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(testObject.NullableReadOnlyObjectCollection, x => MapToTestObjectNestedDto(x)));
            }

            if (testObject.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObjectDto(testObject.SubObject);
            }

            target.IntValue = testObject.IntValue;
            target.StringValue = testObject.StringValue;
            target.RenamedStringValue2 = testObject.RenamedStringValue;
            target.FlatteningIdValue = testObject.Flattening.IdValue;
            target.NullableFlatteningIdValue = testObject.NullableFlattening?.IdValue;
            target.SourceTargetSameObjectType = testObject.SourceTargetSameObjectType;
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)testObject.EnumValue;
            target.EnumName = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName)testObject.EnumName;
            target.EnumRawValue = (byte)testObject.EnumRawValue;
            target.EnumStringValue = MapToString(testObject.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(testObject.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = System.DateOnly.FromDateTime(testObject.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = System.TimeOnly.FromDateTime(testObject.DateTimeValueTargetTimeOnly);
            return target;
        }

        private static Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto MapToTestObjectNestedDto(Riok.Mapperly.IntegrationTests.Models.TestObjectNested source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto();
            target.IntValue = source.IntValue;
            return target;
        }

        private static string MapToString(Riok.Mapperly.IntegrationTests.Models.TestEnum source)
        {
            return source switch
            {
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10),
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20),
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30),
                _ => source.ToString(),
            };
        }

        private static Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue MapToTestEnumDtoByValue(string source)
        {
            return source switch
            {
                nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1) => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1,
                nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2) => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2,
                nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3) => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3,
                _ => System.Enum.Parse<Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue>(source, false),
            };
        }

        private static Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto MapToInheritanceSubObjectDto(Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto();
            target.SubIntValue = source.SubIntValue;
            target.BaseIntValue = source.BaseIntValue;
            return target;
        }
    }
}
