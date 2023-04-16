#nullable enable
namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static partial class ProjectionMapper
    {
        public static partial global::System.Linq.IQueryable<global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection> ProjectToDto(this global::System.Linq.IQueryable<global::Riok.Mapperly.IntegrationTests.Models.TestObjectProjection> q)
        {
#nullable disable
            return System.Linq.Queryable.Select(q, x => new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection(x.CtorValue) { IntValue = x.IntValue, IntInitOnlyValue = x.IntInitOnlyValue, RequiredValue = x.RequiredValue, StringValue = x.StringValue, RenamedStringValue2 = x.RenamedStringValue, FlatteningIdValue = x.Flattening.IdValue, NullableFlatteningIdValue = x.NullableFlattening != null ? x.NullableFlattening.IdValue : default, NestedNullableIntValue = x.NestedNullable != null ? x.NestedNullable.IntValue : default, NestedNullable = x.NestedNullable != null ? new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto() { IntValue = x.NestedNullable.IntValue } : default, NestedNullableTargetNotNullable = x.NestedNullableTargetNotNullable != null ? new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto() { IntValue = x.NestedNullableTargetNotNullable.IntValue } : new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto(), StringNullableTargetNotNullable = x.StringNullableTargetNotNullable ?? "", SourceTargetSameObjectType = x.SourceTargetSameObjectType, NullableReadOnlyObjectCollection = x.NullableReadOnlyObjectCollection != null ? global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Select(x.NullableReadOnlyObjectCollection, x1 => new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto() { IntValue = x1.IntValue })) : default, EnumValue = (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)x.EnumValue, EnumName = (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName)x.EnumName, EnumRawValue = (byte)x.EnumRawValue, EnumStringValue = (string)x.EnumStringValue.ToString(), EnumReverseStringValue = (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName)System.Enum.Parse(typeof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName), x.EnumReverseStringValue, false), SubObject = x.SubObject != null ? new global::Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto() { SubIntValue = x.SubObject.SubIntValue, BaseIntValue = x.SubObject.BaseIntValue } : default, DateTimeValueTargetDateOnly = global::System.DateOnly.FromDateTime(x.DateTimeValueTargetDateOnly), DateTimeValueTargetTimeOnly = global::System.TimeOnly.FromDateTime(x.DateTimeValueTargetTimeOnly), ManuallyMapped = MapManual(x.ManuallyMapped) });
#nullable enable
        }

        private static partial global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection ProjectToDto(this global::Riok.Mapperly.IntegrationTests.Models.TestObjectProjection testObject)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDtoProjection(testObject.CtorValue)
            {
                IntInitOnlyValue = testObject.IntInitOnlyValue,
                RequiredValue = testObject.RequiredValue
            };
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
                target.NullableReadOnlyObjectCollection = global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Select(testObject.NullableReadOnlyObjectCollection, x => MapToTestObjectNestedDto(x)));
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
            target.EnumValue = (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)testObject.EnumValue;
            target.EnumName = (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName)testObject.EnumName;
            target.EnumRawValue = (byte)testObject.EnumRawValue;
            target.EnumStringValue = MapToString(testObject.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByName(testObject.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = global::System.DateOnly.FromDateTime(testObject.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = global::System.TimeOnly.FromDateTime(testObject.DateTimeValueTargetTimeOnly);
            target.ManuallyMapped = MapManual(testObject.ManuallyMapped);
            return target;
        }

        private static global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto MapToTestObjectNestedDto(global::Riok.Mapperly.IntegrationTests.Models.TestObjectNested source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto();
            target.IntValue = source.IntValue;
            return target;
        }

        private static string MapToString(global::Riok.Mapperly.IntegrationTests.Models.TestEnum source)
        {
            return source switch
            {
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10),
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20),
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30),
                _ => source.ToString(),
            };
        }

        private static global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName MapToTestEnumDtoByName(string source)
        {
            return source switch
            {
                nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value10) => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value10,
                nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value20) => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value20,
                nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value30) => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value30,
                _ => (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName)System.Enum.Parse(typeof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName), source, false),
            };
        }

        private static global::Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto MapToInheritanceSubObjectDto(global::Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto();
            target.SubIntValue = source.SubIntValue;
            target.BaseIntValue = source.BaseIntValue;
            return target;
        }
    }
}