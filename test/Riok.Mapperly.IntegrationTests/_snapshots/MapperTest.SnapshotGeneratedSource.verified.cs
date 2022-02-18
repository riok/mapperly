#nullable enable
namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public partial class TestMapper
    {
        public partial int DirectInt(int value)
        {
            return value;
        }

        public partial long ImplicitCastInt(int value)
        {
            return (long)value;
        }

        public partial int ExplicitCastInt(uint value)
        {
            return (int)value;
        }

        public partial int? CastIntNullable(int value)
        {
            return value;
        }

        public partial System.Guid ParseableGuid(string id)
        {
            return System.Guid.Parse(id);
        }

        public partial int ParseableInt(string value)
        {
            return int.Parse(value);
        }

        public partial System.DateTime DirectDateTime(System.DateTime dateTime)
        {
            return dateTime;
        }

        public partial System.Collections.Generic.IEnumerable<Riok.Mapperly.IntegrationTests.Dto.TestObjectDto> MapAllDtos(System.Collections.Generic.IEnumerable<Riok.Mapperly.IntegrationTests.Models.TestObject> objects)
        {
            return System.Linq.Enumerable.Select(objects, x => MapToDto(x));
        }

        public partial Riok.Mapperly.IntegrationTests.Models.TestObject MapFromDto(Riok.Mapperly.IntegrationTests.Dto.TestObjectDto dto)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.TestObject();
            target.IntValue = DirectInt(dto.IntValue);
            target.StringValue = dto.StringValue;
            if (dto.NestedNullable != null)
                target.NestedNullable = MapToTestObjectNested(dto.NestedNullable);
            target.NestedNullableTargetNotNullable = MapToTestObjectNested(dto.NestedNullableTargetNotNullable);
            target.StringNullableTargetNotNullable = dto.StringNullableTargetNotNullable;
            if (dto.RecursiveObject != null)
                target.RecursiveObject = MapFromDto(dto.RecursiveObject);
            target.SourceTargetSameObjectType = dto.SourceTargetSameObjectType;
            if (dto.NullableReadOnlyObjectCollection != null)
                target.NullableReadOnlyObjectCollection = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(dto.NullableReadOnlyObjectCollection, x => MapToTestObjectNested(x)));
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumValue;
            target.EnumName = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumName;
            target.EnumRawValue = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumRawValue;
            target.EnumStringValue = MapToTestEnum(dto.EnumStringValue);
            target.EnumReverseStringValue = MapToString1(dto.EnumReverseStringValue);
            if (dto.SubObject != null)
                target.SubObject = MapToInheritanceSubObject(dto.SubObject);
            return target;
        }

        public partial Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName MapToEnumDtoByName(Riok.Mapperly.IntegrationTests.Models.TestEnum v)
        {
            return v switch
            {
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value10,
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value20,
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value30,
                _ => throw new System.ArgumentOutOfRangeException(nameof(v)),
            };
        }

        private Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto MapToTestObjectNestedDto(Riok.Mapperly.IntegrationTests.Models.TestObjectNested source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto();
            target.IntValue = DirectInt(source.IntValue);
            return target;
        }

        private string MapToString(Riok.Mapperly.IntegrationTests.Models.TestEnum source)
        {
            return source switch
            {
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10),
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20),
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30),
                _ => source.ToString(),
            };
        }

        private Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue MapToTestEnumDtoByValue(string source)
        {
            return source switch
            {
                nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1) => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1,
                nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2) => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2,
                nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3) => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3,
                _ => (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)Enum.Parse(typeof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue), source, false),
            };
        }

        private Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto MapToInheritanceSubObjectDto(Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto();
            target.SubIntValue = DirectInt(source.SubIntValue);
            target.BaseIntValue = DirectInt(source.BaseIntValue);
            return target;
        }

        private Riok.Mapperly.IntegrationTests.Models.TestObjectNested MapToTestObjectNested(Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.TestObjectNested();
            target.IntValue = DirectInt(source.IntValue);
            return target;
        }

        private Riok.Mapperly.IntegrationTests.Models.TestEnum MapToTestEnum(string source)
        {
            return source switch
            {
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10,
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20,
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30,
                _ => (Riok.Mapperly.IntegrationTests.Models.TestEnum)Enum.Parse(typeof(Riok.Mapperly.IntegrationTests.Models.TestEnum), source, false),
            };
        }

        private string MapToString1(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue source)
        {
            return source switch
            {
                Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1 => nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1),
                Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2 => nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2),
                Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3 => nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3),
                _ => source.ToString(),
            };
        }

        private Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject MapToInheritanceSubObject(Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject();
            target.SubIntValue = DirectInt(source.SubIntValue);
            target.BaseIntValue = DirectInt(source.BaseIntValue);
            return target;
        }

        private partial Riok.Mapperly.IntegrationTests.Dto.TestObjectDto MapToDtoInternal(Riok.Mapperly.IntegrationTests.Models.TestObject testObject)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.TestObjectDto();
            target.IntValue = DirectInt(testObject.IntValue);
            target.StringValue = testObject.StringValue;
            target.RenamedStringValue2 = testObject.RenamedStringValue;
            if (testObject.NestedNullable != null)
                target.NestedNullable = MapToTestObjectNestedDto(testObject.NestedNullable);
            if (testObject.NestedNullableTargetNotNullable != null)
                target.NestedNullableTargetNotNullable = MapToTestObjectNestedDto(testObject.NestedNullableTargetNotNullable);
            if (testObject.StringNullableTargetNotNullable != null)
                target.StringNullableTargetNotNullable = testObject.StringNullableTargetNotNullable;
            if (testObject.RecursiveObject != null)
                target.RecursiveObject = MapToDto(testObject.RecursiveObject);
            target.SourceTargetSameObjectType = testObject.SourceTargetSameObjectType;
            if (testObject.NullableReadOnlyObjectCollection != null)
                target.NullableReadOnlyObjectCollection = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(testObject.NullableReadOnlyObjectCollection, x => MapToTestObjectNestedDto(x)));
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)testObject.EnumValue;
            target.EnumName = MapToEnumDtoByName(testObject.EnumName);
            target.EnumRawValue = (byte)testObject.EnumRawValue;
            target.EnumStringValue = MapToString(testObject.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(testObject.EnumReverseStringValue);
            if (testObject.SubObject != null)
                target.SubObject = MapToInheritanceSubObjectDto(testObject.SubObject);
            return target;
        }

        public partial void UpdateDto(Riok.Mapperly.IntegrationTests.Models.TestObject source, Riok.Mapperly.IntegrationTests.Dto.TestObjectDto target)
        {
            target.IntValue = DirectInt(source.IntValue);
            target.StringValue = source.StringValue;
            if (source.NestedNullable != null)
                target.NestedNullable = MapToTestObjectNestedDto(source.NestedNullable);
            if (source.NestedNullableTargetNotNullable != null)
                target.NestedNullableTargetNotNullable = MapToTestObjectNestedDto(source.NestedNullableTargetNotNullable);
            if (source.StringNullableTargetNotNullable != null)
                target.StringNullableTargetNotNullable = source.StringNullableTargetNotNullable;
            if (source.RecursiveObject != null)
                target.RecursiveObject = MapToDto(source.RecursiveObject);
            target.SourceTargetSameObjectType = source.SourceTargetSameObjectType;
            if (source.NullableReadOnlyObjectCollection != null)
                target.NullableReadOnlyObjectCollection = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(source.NullableReadOnlyObjectCollection, x => MapToTestObjectNestedDto(x)));
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)source.EnumValue;
            target.EnumName = MapToEnumDtoByName(source.EnumName);
            target.EnumRawValue = (byte)source.EnumRawValue;
            target.EnumStringValue = MapToString(source.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(source.EnumReverseStringValue);
            if (source.SubObject != null)
                target.SubObject = MapToInheritanceSubObjectDto(source.SubObject);
            target.IgnoredStringValue = source.IgnoredStringValue;
        }

        private partial int PrivateDirectInt(int value)
        {
            return value;
        }
    }
}