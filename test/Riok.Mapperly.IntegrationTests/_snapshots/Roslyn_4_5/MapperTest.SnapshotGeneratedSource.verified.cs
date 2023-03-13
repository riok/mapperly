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
            return (int?)value;
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

        private partial Riok.Mapperly.IntegrationTests.Dto.TestObjectDto MapToDtoInternal(Riok.Mapperly.IntegrationTests.Models.TestObject testObject)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.TestObjectDto(DirectInt(testObject.CtorValue), ctorValue2: DirectInt(testObject.CtorValue2))
            {
                IntInitOnlyValue = DirectInt(testObject.IntInitOnlyValue),
                RequiredValue = DirectInt(testObject.RequiredValue)
            };
            if (testObject.NullableFlattening != null)
            {
                target.NullableFlatteningIdValue = CastIntNullable(testObject.NullableFlattening.IdValue);
            }

            if (testObject.NullableUnflatteningIdValue != null)
            {
                target.NullableUnflattening ??= new();
                target.NullableUnflattening.IdValue = DirectInt(testObject.NullableUnflatteningIdValue.Value);
            }

            if (testObject.NestedNullable != null)
            {
                target.NestedNullableIntValue = DirectInt(testObject.NestedNullable.IntValue);
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

            if (testObject.RecursiveObject != null)
            {
                target.RecursiveObject = MapToDto(testObject.RecursiveObject);
            }

            if (testObject.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(testObject.NullableReadOnlyObjectCollection, x => MapToTestObjectNestedDto(x)));
            }

            if (testObject.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObjectDto(testObject.SubObject);
            }

            target.IntValue = DirectInt(testObject.IntValue);
            target.StringValue = testObject.StringValue;
            target.RenamedStringValue2 = testObject.RenamedStringValue;
            target.FlatteningIdValue = DirectInt(testObject.Flattening.IdValue);
            target.Unflattening.IdValue = DirectInt(testObject.UnflatteningIdValue);
            target.SourceTargetSameObjectType = testObject.SourceTargetSameObjectType;
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)testObject.EnumValue;
            target.EnumName = MapToEnumDtoByName(testObject.EnumName);
            target.EnumRawValue = (byte)testObject.EnumRawValue;
            target.EnumStringValue = MapToString(testObject.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(testObject.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = System.DateOnly.FromDateTime(testObject.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = System.TimeOnly.FromDateTime(testObject.DateTimeValueTargetTimeOnly);
            return target;
        }

        public partial Riok.Mapperly.IntegrationTests.Models.TestObject MapFromDto(Riok.Mapperly.IntegrationTests.Dto.TestObjectDto dto)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.TestObject(DirectInt(dto.CtorValue), ctorValue2: DirectInt(dto.CtorValue2))
            {
                IntInitOnlyValue = DirectInt(dto.IntInitOnlyValue),
                RequiredValue = DirectInt(dto.RequiredValue)
            };
            if (dto.NullableUnflattening != null)
            {
                target.NullableUnflatteningIdValue = CastIntNullable(dto.NullableUnflattening.IdValue);
            }

            if (dto.NestedNullable != null)
            {
                target.NestedNullable = MapToTestObjectNested(dto.NestedNullable);
            }

            if (dto.RecursiveObject != null)
            {
                target.RecursiveObject = MapFromDto(dto.RecursiveObject);
            }

            if (dto.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = MapToIReadOnlyCollection(dto.NullableReadOnlyObjectCollection);
            }

            if (dto.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObject(dto.SubObject);
            }

            target.IntValue = DirectInt(dto.IntValue);
            target.StringValue = dto.StringValue;
            target.UnflatteningIdValue = DirectInt(dto.Unflattening.IdValue);
            target.NestedNullableTargetNotNullable = MapToTestObjectNested(dto.NestedNullableTargetNotNullable);
            target.StringNullableTargetNotNullable = dto.StringNullableTargetNotNullable;
            target.SourceTargetSameObjectType = dto.SourceTargetSameObjectType;
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumValue;
            target.EnumName = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumName;
            target.EnumRawValue = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumRawValue;
            target.EnumStringValue = MapToTestEnum(dto.EnumStringValue);
            target.EnumReverseStringValue = MapToString1(dto.EnumReverseStringValue);
            return target;
        }

        public partial Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName MapToEnumDtoByName(Riok.Mapperly.IntegrationTests.Models.TestEnum v)
        {
            return v switch
            {
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value10,
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value20,
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value30,
                _ => throw new System.ArgumentOutOfRangeException(nameof(v), v, "The value of enum TestEnum is not supported"),
            };
        }

        public partial void UpdateDto(Riok.Mapperly.IntegrationTests.Models.TestObject source, Riok.Mapperly.IntegrationTests.Dto.TestObjectDto target)
        {
            if (source.NullableFlattening != null)
            {
                target.NullableFlatteningIdValue = CastIntNullable(source.NullableFlattening.IdValue);
            }

            if (source.NestedNullable != null)
            {
                target.NestedNullableIntValue = DirectInt(source.NestedNullable.IntValue);
                target.NestedNullable = MapToTestObjectNestedDto(source.NestedNullable);
            }

            if (source.NestedNullableTargetNotNullable != null)
            {
                target.NestedNullableTargetNotNullable = MapToTestObjectNestedDto(source.NestedNullableTargetNotNullable);
            }

            if (source.StringNullableTargetNotNullable != null)
            {
                target.StringNullableTargetNotNullable = source.StringNullableTargetNotNullable;
            }

            if (source.RecursiveObject != null)
            {
                target.RecursiveObject = MapToDto(source.RecursiveObject);
            }

            if (source.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(source.NullableReadOnlyObjectCollection, x => MapToTestObjectNestedDto(x)));
            }

            if (source.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObjectDto(source.SubObject);
            }

            target.CtorValue = DirectInt(source.CtorValue);
            target.CtorValue2 = DirectInt(source.CtorValue2);
            target.IntValue = DirectInt(source.IntValue);
            target.StringValue = source.StringValue;
            target.FlatteningIdValue = DirectInt(source.Flattening.IdValue);
            target.SourceTargetSameObjectType = source.SourceTargetSameObjectType;
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)source.EnumValue;
            target.EnumName = MapToEnumDtoByName(source.EnumName);
            target.EnumRawValue = (byte)source.EnumRawValue;
            target.EnumStringValue = MapToString(source.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(source.EnumReverseStringValue);
            target.IgnoredStringValue = source.IgnoredStringValue;
            target.DateTimeValueTargetDateOnly = System.DateOnly.FromDateTime(source.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = System.TimeOnly.FromDateTime(source.DateTimeValueTargetTimeOnly);
        }

        private partial int PrivateDirectInt(int value)
        {
            return value;
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
                _ => (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)System.Enum.Parse(typeof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue), source, false),
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

        private System.Collections.Generic.IReadOnlyCollection<Riok.Mapperly.IntegrationTests.Models.TestObjectNested> MapToIReadOnlyCollection(Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto[] source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.TestObjectNested[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                target[i] = MapToTestObjectNested(source[i]);
            }

            return target;
        }

        private Riok.Mapperly.IntegrationTests.Models.TestEnum MapToTestEnum(string source)
        {
            return source switch
            {
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10,
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20,
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30,
                _ => (Riok.Mapperly.IntegrationTests.Models.TestEnum)System.Enum.Parse(typeof(Riok.Mapperly.IntegrationTests.Models.TestEnum), source, false),
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
    }
}