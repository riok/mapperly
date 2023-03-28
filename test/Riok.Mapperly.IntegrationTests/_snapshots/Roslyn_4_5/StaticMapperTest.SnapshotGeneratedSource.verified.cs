#nullable enable
namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static partial class StaticTestMapper
    {
        public static partial int DirectInt(int value)
        {
            return value;
        }

        public static partial long ImplicitCastInt(int value)
        {
            return (long)value;
        }

        public static partial int ExplicitCastInt(uint value)
        {
            return (int)value;
        }

        public static partial int? CastIntNullable(int value)
        {
            return (int?)value;
        }

        public static partial System.Guid ParseableGuid(string id)
        {
            return System.Guid.Parse(id);
        }

        public static partial int ParseableInt(string value)
        {
            return int.Parse(value);
        }

        public static partial System.DateTime DirectDateTime(System.DateTime dateTime)
        {
            return dateTime;
        }

        public static partial System.Collections.Generic.IEnumerable<Riok.Mapperly.IntegrationTests.Dto.TestObjectDto> MapAllDtos(System.Collections.Generic.IEnumerable<Riok.Mapperly.IntegrationTests.Models.TestObject> objects)
        {
            return System.Linq.Enumerable.Select(objects, x => MapToDtoExt(x));
        }

        public static partial Riok.Mapperly.IntegrationTests.Dto.TestObjectDto MapToDtoExt(this Riok.Mapperly.IntegrationTests.Models.TestObject src)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.TestObjectDto(DirectInt(src.CtorValue), ctorValue2: DirectInt(src.CtorValue2))
            {
                IntInitOnlyValue = DirectInt(src.IntInitOnlyValue),
                RequiredValue = DirectInt(src.RequiredValue)
            };
            if (src.NullableFlattening != null)
            {
                target.NullableFlatteningIdValue = CastIntNullable(src.NullableFlattening.IdValue);
            }

            if (src.NestedNullable != null)
            {
                target.NestedNullableIntValue = DirectInt(src.NestedNullable.IntValue);
                target.NestedNullable = MapToTestObjectNestedDto(src.NestedNullable);
            }

            if (src.NestedNullableTargetNotNullable != null)
            {
                target.NestedNullableTargetNotNullable = MapToTestObjectNestedDto(src.NestedNullableTargetNotNullable);
            }

            if (src.StringNullableTargetNotNullable != null)
            {
                target.StringNullableTargetNotNullable = src.StringNullableTargetNotNullable;
            }

            if (src.RecursiveObject != null)
            {
                target.RecursiveObject = MapToDtoExt(src.RecursiveObject);
            }

            if (src.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(src.NullableReadOnlyObjectCollection, x => MapToTestObjectNestedDto(x)));
            }

            if (src.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObjectDto(src.SubObject);
            }

            target.IntValue = DirectInt(src.IntValue);
            target.StringValue = src.StringValue;
            target.FlatteningIdValue = DirectInt(src.Flattening.IdValue);
            target.SourceTargetSameObjectType = src.SourceTargetSameObjectType;
            target.StackValue = MapToStack(src.StackValue);
            target.QueueValue = MapToQueue(src.QueueValue);
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)src.EnumValue;
            target.EnumName = MapToEnumDtoByName(src.EnumName);
            target.EnumRawValue = (byte)src.EnumRawValue;
            target.EnumStringValue = MapToString(src.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(src.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = System.DateOnly.FromDateTime(src.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = System.TimeOnly.FromDateTime(src.DateTimeValueTargetTimeOnly);
            return target;
        }

        private static partial Riok.Mapperly.IntegrationTests.Dto.TestObjectDto MapToDtoInternal(Riok.Mapperly.IntegrationTests.Models.TestObject testObject)
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
                target.RecursiveObject = MapToDtoExt(testObject.RecursiveObject);
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
            target.StackValue = MapToStack(testObject.StackValue);
            target.QueueValue = MapToQueue(testObject.QueueValue);
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)testObject.EnumValue;
            target.EnumName = MapToEnumDtoByName(testObject.EnumName);
            target.EnumRawValue = (byte)testObject.EnumRawValue;
            target.EnumStringValue = MapToString(testObject.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(testObject.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = System.DateOnly.FromDateTime(testObject.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = System.TimeOnly.FromDateTime(testObject.DateTimeValueTargetTimeOnly);
            return target;
        }

        public static partial Riok.Mapperly.IntegrationTests.Models.TestObject MapFromDto(Riok.Mapperly.IntegrationTests.Dto.TestObjectDto dto)
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
            target.StackValue = MapToStack1(dto.StackValue);
            target.QueueValue = MapToQueue1(dto.QueueValue);
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumValue;
            target.EnumName = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumName;
            target.EnumRawValue = (Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumRawValue;
            target.EnumStringValue = MapToTestEnum(dto.EnumStringValue);
            target.EnumReverseStringValue = MapToString1(dto.EnumReverseStringValue);
            return target;
        }

        public static partial Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName MapToEnumDtoByName(Riok.Mapperly.IntegrationTests.Models.TestEnum v)
        {
            return v switch
            {
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value10,
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value20,
                Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value30,
                _ => throw new System.ArgumentOutOfRangeException(nameof(v), v, "The value of enum TestEnum is not supported"),
            };
        }

        public static partial void UpdateDto(Riok.Mapperly.IntegrationTests.Models.TestObject source, Riok.Mapperly.IntegrationTests.Dto.TestObjectDto target)
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
                target.RecursiveObject = MapToDtoExt(source.RecursiveObject);
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
            target.StackValue = MapToStack(source.StackValue);
            target.QueueValue = MapToQueue(source.QueueValue);
            target.EnumValue = (Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)source.EnumValue;
            target.EnumName = MapToEnumDtoByName(source.EnumName);
            target.EnumRawValue = (byte)source.EnumRawValue;
            target.EnumStringValue = MapToString(source.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(source.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = System.DateOnly.FromDateTime(source.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = System.TimeOnly.FromDateTime(source.DateTimeValueTargetTimeOnly);
        }

        private static partial int PrivateDirectInt(int value)
        {
            return value;
        }

        private static Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto MapToTestObjectNestedDto(Riok.Mapperly.IntegrationTests.Models.TestObjectNested source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto();
            target.IntValue = DirectInt(source.IntValue);
            return target;
        }

        private static System.Collections.Generic.Stack<int> MapToStack(System.Collections.Generic.Stack<string> source)
        {
            var target = new System.Collections.Generic.Stack<int>();
            foreach (var item in source)
            {
                target.Push(ParseableInt(item));
            }

            return target;
        }

        private static System.Collections.Generic.Queue<int> MapToQueue(System.Collections.Generic.Queue<string> source)
        {
            var target = new System.Collections.Generic.Queue<int>();
            foreach (var item in source)
            {
                target.Enqueue(ParseableInt(item));
            }

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
            target.SubIntValue = DirectInt(source.SubIntValue);
            target.BaseIntValue = DirectInt(source.BaseIntValue);
            return target;
        }

        private static Riok.Mapperly.IntegrationTests.Models.TestObjectNested MapToTestObjectNested(Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.TestObjectNested();
            target.IntValue = DirectInt(source.IntValue);
            return target;
        }

        private static System.Collections.Generic.IReadOnlyCollection<Riok.Mapperly.IntegrationTests.Models.TestObjectNested> MapToIReadOnlyCollection(Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto[] source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.TestObjectNested[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                target[i] = MapToTestObjectNested(source[i]);
            }

            return target;
        }

        private static System.Collections.Generic.Stack<string> MapToStack1(System.Collections.Generic.Stack<int> source)
        {
            var target = new System.Collections.Generic.Stack<string>();
            foreach (var item in source)
            {
                target.Push(item.ToString());
            }

            return target;
        }

        private static System.Collections.Generic.Queue<string> MapToQueue1(System.Collections.Generic.Queue<int> source)
        {
            var target = new System.Collections.Generic.Queue<string>();
            foreach (var item in source)
            {
                target.Enqueue(item.ToString());
            }

            return target;
        }

        private static Riok.Mapperly.IntegrationTests.Models.TestEnum MapToTestEnum(string source)
        {
            return source switch
            {
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10,
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20,
                nameof(Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30) => Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30,
                _ => System.Enum.Parse<Riok.Mapperly.IntegrationTests.Models.TestEnum>(source, false),
            };
        }

        private static string MapToString1(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue source)
        {
            return source switch
            {
                Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1 => nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1),
                Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2 => nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2),
                Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3 => nameof(Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3),
                _ => source.ToString(),
            };
        }

        private static Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject MapToInheritanceSubObject(Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto source)
        {
            var target = new Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject();
            target.SubIntValue = DirectInt(source.SubIntValue);
            target.BaseIntValue = DirectInt(source.BaseIntValue);
            return target;
        }
    }
}