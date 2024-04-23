﻿// <auto-generated />
#nullable enable
namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public partial class TestMapper
    {
        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial int DirectInt(int value)
        {
            return value;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial long ImplicitCastInt(int value)
        {
            return (long)value;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial int ExplicitCastInt(uint value)
        {
            return (int)value;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial int? CastIntNullable(int value)
        {
            return (int?)value;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial global::System.Guid ParseableGuid(string id)
        {
            return global::System.Guid.Parse(id);
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial int ParseableInt(string value)
        {
            return int.Parse(value);
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial global::System.DateTime DirectDateTime(global::System.DateTime dateTime)
        {
            return dateTime;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial global::System.Collections.Generic.IEnumerable<global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDto> MapAllDtos(global::System.Collections.Generic.IEnumerable<global::Riok.Mapperly.IntegrationTests.Models.TestObject> objects)
        {
            return global::System.Linq.Enumerable.Select(objects, x => MapToDto(x));
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private partial global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDto MapToDtoInternal(global::Riok.Mapperly.IntegrationTests.Models.TestObject testObject)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDto(DirectInt(testObject.CtorValue), ctorValue2: DirectInt(testObject.CtorValue2))
            {
                IntInitOnlyValue = DirectInt(testObject.IntInitOnlyValue),
                RequiredValue = DirectInt(testObject.RequiredValue),
            };
            if (testObject.NullableFlattening != null)
            {
                target.NullableFlatteningIdValue = CastIntNullable(testObject.NullableFlattening.IdValue);
            }
            else
            {
                target.NullableFlatteningIdValue = null;
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
            else
            {
                target.NestedNullable = null;
            }
            if (testObject.NestedNullableTargetNotNullable != null)
            {
                target.NestedNullableTargetNotNullable = MapToTestObjectNestedDto(testObject.NestedNullableTargetNotNullable);
            }
            if (testObject.NestedMember != null)
            {
                if (testObject.NestedMember.NestedMemberObject != null)
                {
                    target.NestedMemberObjectIntValue = DirectInt(testObject.NestedMember.NestedMemberObject.IntValue);
                }
                target.NestedMemberId = DirectInt(testObject.NestedMember.NestedMemberId);
            }
            if (testObject.StringNullableTargetNotNullable != null)
            {
                target.StringNullableTargetNotNullable = testObject.StringNullableTargetNotNullable;
            }
            if (testObject.TupleValue != null)
            {
                target.TupleValue = MapToValueTupleOfInt32AndInt32(testObject.TupleValue.Value);
            }
            else
            {
                target.TupleValue = null;
            }
            if (testObject.RecursiveObject != null)
            {
                target.RecursiveObject = MapToDto(testObject.RecursiveObject);
            }
            else
            {
                target.RecursiveObject = null;
            }
            if (testObject.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = MapToTestObjectNestedDtoArray(testObject.NullableReadOnlyObjectCollection);
            }
            else
            {
                target.NullableReadOnlyObjectCollection = null;
            }
            if (testObject.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObjectDto(testObject.SubObject);
            }
            else
            {
                target.SubObject = null;
            }
            target.IntValue = DirectInt(testObject.IntValue);
            target.StringValue = testObject.StringValue;
            target.RenamedStringValue2 = testObject.RenamedStringValue;
            target.FlatteningIdValue = DirectInt(testObject.Flattening.IdValue);
            target.Unflattening.IdValue = DirectInt(testObject.UnflatteningIdValue);
            target.SourceTargetSameObjectType = testObject.SourceTargetSameObjectType;
            target.SpanValue = MapToInt32Array(testObject.SpanValue);
            target.MemoryValue = MapToInt32Array1(testObject.MemoryValue.Span);
            target.StackValue = new global::System.Collections.Generic.Stack<int>(global::System.Linq.Enumerable.Select(testObject.StackValue, x => ParseableInt(x)));
            target.QueueValue = new global::System.Collections.Generic.Queue<int>(global::System.Linq.Enumerable.Select(testObject.QueueValue, x => ParseableInt(x)));
            target.ImmutableArrayValue = global::System.Collections.Immutable.ImmutableArray.ToImmutableArray(global::System.Linq.Enumerable.Select(testObject.ImmutableArrayValue, x => ParseableInt(x)));
            target.ImmutableListValue = global::System.Collections.Immutable.ImmutableList.ToImmutableList(global::System.Linq.Enumerable.Select(testObject.ImmutableListValue, x => ParseableInt(x)));
            target.ImmutableHashSetValue = global::System.Collections.Immutable.ImmutableHashSet.ToImmutableHashSet(global::System.Linq.Enumerable.Select(testObject.ImmutableHashSetValue, x => ParseableInt(x)));
            target.ImmutableQueueValue = global::System.Collections.Immutable.ImmutableQueue.CreateRange(global::System.Linq.Enumerable.Select(testObject.ImmutableQueueValue, x => ParseableInt(x)));
            target.ImmutableStackValue = global::System.Collections.Immutable.ImmutableStack.CreateRange(global::System.Linq.Enumerable.Select(testObject.ImmutableStackValue, x => ParseableInt(x)));
            target.ImmutableSortedSetValue = global::System.Collections.Immutable.ImmutableSortedSet.ToImmutableSortedSet(global::System.Linq.Enumerable.Select(testObject.ImmutableSortedSetValue, x => ParseableInt(x)));
            target.ImmutableDictionaryValue = global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(testObject.ImmutableDictionaryValue, x => ParseableInt(x.Key), x => ParseableInt(x.Value));
            target.ImmutableSortedDictionaryValue = global::System.Collections.Immutable.ImmutableSortedDictionary.ToImmutableSortedDictionary(testObject.ImmutableSortedDictionaryValue, x => ParseableInt(x.Key), x => ParseableInt(x.Value));
            foreach (var item in testObject.ExistingISet)
            {
                target.ExistingISet.Add(ParseableInt(item));
            }
            target.ExistingHashSet.EnsureCapacity(testObject.ExistingHashSet.Count + target.ExistingHashSet.Count);
            foreach (var item1 in testObject.ExistingHashSet)
            {
                target.ExistingHashSet.Add(ParseableInt(item1));
            }
            foreach (var item2 in testObject.ExistingSortedSet)
            {
                target.ExistingSortedSet.Add(ParseableInt(item2));
            }
            target.ExistingList.EnsureCapacity(testObject.ExistingList.Count + target.ExistingList.Count);
            foreach (var item3 in testObject.ExistingList)
            {
                target.ExistingList.Add(ParseableInt(item3));
            }
            target.ISet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(testObject.ISet, x => ParseableInt(x)));
            target.IReadOnlySet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(testObject.IReadOnlySet, x => ParseableInt(x)));
            target.HashSet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(testObject.HashSet, x => ParseableInt(x)));
            target.SortedSet = new global::System.Collections.Generic.SortedSet<int>(global::System.Linq.Enumerable.Select(testObject.SortedSet, x => ParseableInt(x)));
            target.EnumValue = (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)testObject.EnumValue;
            target.FlagsEnumValue = (global::Riok.Mapperly.IntegrationTests.Dto.TestFlagsEnumDto)testObject.FlagsEnumValue;
            target.EnumName = MapToEnumDtoByName(testObject.EnumName);
            target.EnumRawValue = (byte)testObject.EnumRawValue;
            target.EnumStringValue = MapToString(testObject.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(testObject.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = global::System.DateOnly.FromDateTime(testObject.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = global::System.TimeOnly.FromDateTime(testObject.DateTimeValueTargetTimeOnly);
            target.FormattedIntValue = testObject.IntValue.ToString("C", _formatDeCh);
            target.FormattedDateValue = testObject.DateTimeValue.ToString("D", _formatEnUs);
            target.SetPrivateValue(DirectInt(testObject.GetPrivateValue()));
            target.Sum = ComputeSum(testObject);
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial global::Riok.Mapperly.IntegrationTests.Models.TestObject MapFromDto(global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDto dto)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Models.TestObject(DirectInt(dto.CtorValue), ctorValue2: DirectInt(dto.CtorValue2))
            {
                IntInitOnlyValue = DirectInt(dto.IntInitOnlyValue),
                RequiredValue = DirectInt(dto.RequiredValue),
            };
            if (dto.NullableUnflattening != null)
            {
                target.NullableUnflatteningIdValue = CastIntNullable(dto.NullableUnflattening.IdValue);
            }
            else
            {
                target.NullableUnflatteningIdValue = null;
            }
            if (dto.NestedNullable != null)
            {
                target.NestedNullable = MapToTestObjectNested(dto.NestedNullable);
            }
            else
            {
                target.NestedNullable = null;
            }
            if (dto.TupleValue != null)
            {
                target.TupleValue = MapToValueTupleOfStringAndString(dto.TupleValue.Value);
            }
            else
            {
                target.TupleValue = null;
            }
            if (dto.RecursiveObject != null)
            {
                target.RecursiveObject = MapFromDto(dto.RecursiveObject);
            }
            else
            {
                target.RecursiveObject = null;
            }
            if (dto.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = MapToTestObjectNestedArray(dto.NullableReadOnlyObjectCollection);
            }
            else
            {
                target.NullableReadOnlyObjectCollection = null;
            }
            if (dto.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObject(dto.SubObject);
            }
            else
            {
                target.SubObject = null;
            }
            target.IntValue = DirectInt(dto.IntValue);
            target.StringValue = dto.StringValue;
            target.UnflatteningIdValue = DirectInt(dto.Unflattening.IdValue);
            target.NestedNullableTargetNotNullable = MapToTestObjectNested(dto.NestedNullableTargetNotNullable);
            target.StringNullableTargetNotNullable = dto.StringNullableTargetNotNullable;
            target.SourceTargetSameObjectType = dto.SourceTargetSameObjectType;
            target.MemoryValue = MapToStringArray(dto.MemoryValue.Span);
            target.StackValue = new global::System.Collections.Generic.Stack<string>(global::System.Linq.Enumerable.Select(dto.StackValue, x => x.ToString(_formatDeCh)));
            target.QueueValue = new global::System.Collections.Generic.Queue<string>(global::System.Linq.Enumerable.Select(dto.QueueValue, x => x.ToString(_formatDeCh)));
            target.ImmutableArrayValue = global::System.Collections.Immutable.ImmutableArray.ToImmutableArray(global::System.Linq.Enumerable.Select(dto.ImmutableArrayValue, x => x.ToString(_formatDeCh)));
            target.ImmutableListValue = global::System.Collections.Immutable.ImmutableList.ToImmutableList(global::System.Linq.Enumerable.Select(dto.ImmutableListValue, x => x.ToString(_formatDeCh)));
            target.ImmutableHashSetValue = global::System.Collections.Immutable.ImmutableHashSet.ToImmutableHashSet(global::System.Linq.Enumerable.Select(dto.ImmutableHashSetValue, x => x.ToString(_formatDeCh)));
            target.ImmutableQueueValue = global::System.Collections.Immutable.ImmutableQueue.CreateRange(global::System.Linq.Enumerable.Select(dto.ImmutableQueueValue, x => x.ToString(_formatDeCh)));
            target.ImmutableStackValue = global::System.Collections.Immutable.ImmutableStack.CreateRange(global::System.Linq.Enumerable.Select(dto.ImmutableStackValue, x => x.ToString(_formatDeCh)));
            target.ImmutableSortedSetValue = global::System.Collections.Immutable.ImmutableSortedSet.ToImmutableSortedSet(global::System.Linq.Enumerable.Select(dto.ImmutableSortedSetValue, x => x.ToString(_formatDeCh)));
            target.ImmutableDictionaryValue = global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(dto.ImmutableDictionaryValue, x => x.Key.ToString(_formatDeCh), x => x.Value.ToString(_formatDeCh));
            target.ImmutableSortedDictionaryValue = global::System.Collections.Immutable.ImmutableSortedDictionary.ToImmutableSortedDictionary(dto.ImmutableSortedDictionaryValue, x => x.Key.ToString(_formatDeCh), x => x.Value.ToString(_formatDeCh));
            foreach (var item in dto.ExistingISet)
            {
                target.ExistingISet.Add(item.ToString(_formatDeCh));
            }
            target.ExistingHashSet.EnsureCapacity(dto.ExistingHashSet.Count + target.ExistingHashSet.Count);
            foreach (var item1 in dto.ExistingHashSet)
            {
                target.ExistingHashSet.Add(item1.ToString(_formatDeCh));
            }
            foreach (var item2 in dto.ExistingSortedSet)
            {
                target.ExistingSortedSet.Add(item2.ToString(_formatDeCh));
            }
            target.ExistingList.EnsureCapacity(dto.ExistingList.Count + target.ExistingList.Count);
            foreach (var item3 in dto.ExistingList)
            {
                target.ExistingList.Add(item3.ToString(_formatDeCh));
            }
            target.ISet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(dto.ISet, x => x.ToString(_formatDeCh)));
            target.IReadOnlySet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(dto.IReadOnlySet, x => x.ToString(_formatDeCh)));
            target.HashSet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(dto.HashSet, x => x.ToString(_formatDeCh)));
            target.SortedSet = new global::System.Collections.Generic.SortedSet<string>(global::System.Linq.Enumerable.Select(dto.SortedSet, x => x.ToString(_formatDeCh)));
            target.EnumValue = (global::Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumValue;
            target.FlagsEnumValue = (global::Riok.Mapperly.IntegrationTests.Models.TestFlagsEnum)dto.FlagsEnumValue;
            target.EnumName = (global::Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumName;
            target.EnumRawValue = (global::Riok.Mapperly.IntegrationTests.Models.TestEnum)dto.EnumRawValue;
            target.EnumStringValue = MapToTestEnum(dto.EnumStringValue);
            target.EnumReverseStringValue = MapToString1(dto.EnumReverseStringValue);
            target.SetPrivateValue1(DirectInt(dto.GetPrivateValue1()));
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial void UpdateDto(global::Riok.Mapperly.IntegrationTests.Models.TestObject source, global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDto target)
        {
            if (source.NullableFlattening != null)
            {
                target.NullableFlatteningIdValue = CastIntNullable(source.NullableFlattening.IdValue);
            }
            else
            {
                target.NullableFlatteningIdValue = null;
            }
            if (source.NestedNullable != null)
            {
                target.NestedNullableIntValue = DirectInt(source.NestedNullable.IntValue);
                target.NestedNullable = MapToTestObjectNestedDto(source.NestedNullable);
            }
            else
            {
                target.NestedNullable = null;
            }
            if (source.NestedNullableTargetNotNullable != null)
            {
                target.NestedNullableTargetNotNullable = MapToTestObjectNestedDto(source.NestedNullableTargetNotNullable);
            }
            if (source.StringNullableTargetNotNullable != null)
            {
                target.StringNullableTargetNotNullable = source.StringNullableTargetNotNullable;
            }
            if (source.TupleValue != null)
            {
                target.TupleValue = MapToValueTupleOfInt32AndInt32(source.TupleValue.Value);
            }
            else
            {
                target.TupleValue = null;
            }
            if (source.RecursiveObject != null)
            {
                target.RecursiveObject = MapToDto(source.RecursiveObject);
            }
            else
            {
                target.RecursiveObject = null;
            }
            if (source.NullableReadOnlyObjectCollection != null)
            {
                target.NullableReadOnlyObjectCollection = MapToTestObjectNestedDtoArray(source.NullableReadOnlyObjectCollection);
            }
            else
            {
                target.NullableReadOnlyObjectCollection = null;
            }
            if (source.SubObject != null)
            {
                target.SubObject = MapToInheritanceSubObjectDto(source.SubObject);
            }
            else
            {
                target.SubObject = null;
            }
            target.CtorValue = DirectInt(source.CtorValue);
            target.CtorValue2 = DirectInt(source.CtorValue2);
            target.IntValue = DirectInt(source.IntValue);
            target.StringValue = source.StringValue;
            target.FlatteningIdValue = DirectInt(source.Flattening.IdValue);
            target.SourceTargetSameObjectType = source.SourceTargetSameObjectType;
            target.SpanValue = MapToInt32Array(source.SpanValue);
            target.MemoryValue = MapToInt32Array1(source.MemoryValue.Span);
            target.StackValue = new global::System.Collections.Generic.Stack<int>(global::System.Linq.Enumerable.Select(source.StackValue, x => ParseableInt(x)));
            target.QueueValue = new global::System.Collections.Generic.Queue<int>(global::System.Linq.Enumerable.Select(source.QueueValue, x => ParseableInt(x)));
            target.ImmutableArrayValue = global::System.Collections.Immutable.ImmutableArray.ToImmutableArray(global::System.Linq.Enumerable.Select(source.ImmutableArrayValue, x => ParseableInt(x)));
            target.ImmutableListValue = global::System.Collections.Immutable.ImmutableList.ToImmutableList(global::System.Linq.Enumerable.Select(source.ImmutableListValue, x => ParseableInt(x)));
            target.ImmutableHashSetValue = global::System.Collections.Immutable.ImmutableHashSet.ToImmutableHashSet(global::System.Linq.Enumerable.Select(source.ImmutableHashSetValue, x => ParseableInt(x)));
            target.ImmutableQueueValue = global::System.Collections.Immutable.ImmutableQueue.CreateRange(global::System.Linq.Enumerable.Select(source.ImmutableQueueValue, x => ParseableInt(x)));
            target.ImmutableStackValue = global::System.Collections.Immutable.ImmutableStack.CreateRange(global::System.Linq.Enumerable.Select(source.ImmutableStackValue, x => ParseableInt(x)));
            target.ImmutableSortedSetValue = global::System.Collections.Immutable.ImmutableSortedSet.ToImmutableSortedSet(global::System.Linq.Enumerable.Select(source.ImmutableSortedSetValue, x => ParseableInt(x)));
            target.ImmutableDictionaryValue = global::System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary(source.ImmutableDictionaryValue, x => ParseableInt(x.Key), x => ParseableInt(x.Value));
            target.ImmutableSortedDictionaryValue = global::System.Collections.Immutable.ImmutableSortedDictionary.ToImmutableSortedDictionary(source.ImmutableSortedDictionaryValue, x => ParseableInt(x.Key), x => ParseableInt(x.Value));
            foreach (var item in source.ExistingISet)
            {
                target.ExistingISet.Add(ParseableInt(item));
            }
            target.ExistingHashSet.EnsureCapacity(source.ExistingHashSet.Count + target.ExistingHashSet.Count);
            foreach (var item1 in source.ExistingHashSet)
            {
                target.ExistingHashSet.Add(ParseableInt(item1));
            }
            foreach (var item2 in source.ExistingSortedSet)
            {
                target.ExistingSortedSet.Add(ParseableInt(item2));
            }
            target.ExistingList.EnsureCapacity(source.ExistingList.Count + target.ExistingList.Count);
            foreach (var item3 in source.ExistingList)
            {
                target.ExistingList.Add(ParseableInt(item3));
            }
            target.ISet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(source.ISet, x => ParseableInt(x)));
            target.IReadOnlySet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(source.IReadOnlySet, x => ParseableInt(x)));
            target.HashSet = global::System.Linq.Enumerable.ToHashSet(global::System.Linq.Enumerable.Select(source.HashSet, x => ParseableInt(x)));
            target.SortedSet = new global::System.Collections.Generic.SortedSet<int>(global::System.Linq.Enumerable.Select(source.SortedSet, x => ParseableInt(x)));
            target.EnumValue = (global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue)source.EnumValue;
            target.FlagsEnumValue = (global::Riok.Mapperly.IntegrationTests.Dto.TestFlagsEnumDto)source.FlagsEnumValue;
            target.EnumName = MapToEnumDtoByName(source.EnumName);
            target.EnumRawValue = (byte)source.EnumRawValue;
            target.EnumStringValue = MapToString(source.EnumStringValue);
            target.EnumReverseStringValue = MapToTestEnumDtoByValue(source.EnumReverseStringValue);
            target.DateTimeValueTargetDateOnly = global::System.DateOnly.FromDateTime(source.DateTimeValueTargetDateOnly);
            target.DateTimeValueTargetTimeOnly = global::System.TimeOnly.FromDateTime(source.DateTimeValueTargetTimeOnly);
            target.SetPrivateValue(DirectInt(source.GetPrivateValue()));
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName MapToEnumDtoByName(global::Riok.Mapperly.IntegrationTests.Models.TestEnum v)
        {
            return v switch
            {
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value10,
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value20,
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByName.Value30,
                _ => throw new System.ArgumentOutOfRangeException(nameof(v), v, "The value of enum TestEnum is not supported"),
            };
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private partial int PrivateDirectInt(int value)
        {
            return value;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        public partial (string X, string Y) MapAliasedTuple((int X, int Y) source)
        {
            var target = (X: source.X.ToString(_formatDeCh), Y: source.Y.ToString(_formatDeCh));
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto MapToTestObjectNestedDto(global::Riok.Mapperly.IntegrationTests.Models.TestObjectNested source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto();
            target.IntValue = DirectInt(source.IntValue);
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private (int A, int) MapToValueTupleOfInt32AndInt32((string A, string) source)
        {
            var target = (A: ParseableInt(source.A), ParseableInt(source.Item2));
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto[] MapToTestObjectNestedDtoArray(global::System.Collections.Generic.IReadOnlyCollection<global::Riok.Mapperly.IntegrationTests.Models.TestObjectNested> source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto[source.Count];
            var i = 0;
            foreach (var item in source)
            {
                target[i] = MapToTestObjectNestedDto(item);
                i++;
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private int[] MapToInt32Array(global::System.Span<string> source)
        {
            var target = new int[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                target[i] = ParseableInt(source[i]);
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private int[] MapToInt32Array1(global::System.ReadOnlySpan<string> source)
        {
            var target = new int[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                target[i] = ParseableInt(source[i]);
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private string MapToString(global::Riok.Mapperly.IntegrationTests.Models.TestEnum source)
        {
            return source switch
            {
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10 => nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10),
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20 => nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20),
                global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30 => nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30),
                _ => source.ToString(),
            };
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue MapToTestEnumDtoByValue(string source)
        {
            return source switch
            {
                nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1) => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1,
                nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2) => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2,
                nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3) => global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3,
                _ => System.Enum.Parse<global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue>(source, false),
            };
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto MapToInheritanceSubObjectDto(global::Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto();
            target.SubIntValue = DirectInt(source.SubIntValue);
            target.BaseIntValue = DirectInt(source.BaseIntValue);
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Models.TestObjectNested MapToTestObjectNested(global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Models.TestObjectNested();
            target.IntValue = DirectInt(source.IntValue);
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private (string A, string) MapToValueTupleOfStringAndString((int A, int) source)
        {
            var target = (A: source.A.ToString(_formatDeCh), source.Item2.ToString(_formatDeCh));
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Models.TestObjectNested[] MapToTestObjectNestedArray(global::Riok.Mapperly.IntegrationTests.Dto.TestObjectNestedDto[] source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Models.TestObjectNested[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                target[i] = MapToTestObjectNested(source[i]);
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private string[] MapToStringArray(global::System.ReadOnlySpan<int> source)
        {
            var target = new string[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                target[i] = source[i].ToString(_formatDeCh);
            }
            return target;
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Models.TestEnum MapToTestEnum(string source)
        {
            return source switch
            {
                nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10) => global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value10,
                nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20) => global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value20,
                nameof(global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30) => global::Riok.Mapperly.IntegrationTests.Models.TestEnum.Value30,
                _ => System.Enum.Parse<global::Riok.Mapperly.IntegrationTests.Models.TestEnum>(source, false),
            };
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private string MapToString1(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue source)
        {
            return source switch
            {
                global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1 => nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue1),
                global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2 => nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue2),
                global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3 => nameof(global::Riok.Mapperly.IntegrationTests.Dto.TestEnumDtoByValue.DtoValue3),
                _ => source.ToString(),
            };
        }

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        private global::Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject MapToInheritanceSubObject(global::Riok.Mapperly.IntegrationTests.Dto.InheritanceSubObjectDto source)
        {
            var target = new global::Riok.Mapperly.IntegrationTests.Models.InheritanceSubObject();
            target.SubIntValue = DirectInt(source.SubIntValue);
            target.BaseIntValue = DirectInt(source.BaseIntValue);
            return target;
        }
    }
        
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    static file class UnsafeAccessor
    {
        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "get_PrivateValue")]
        public static extern int GetPrivateValue(this global::Riok.Mapperly.IntegrationTests.Models.TestObject source);

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_PrivateValue")]
        public static extern void SetPrivateValue(this global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDto target, int value);

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "get_PrivateValue")]
        public static extern int GetPrivateValue1(this global::Riok.Mapperly.IntegrationTests.Dto.TestObjectDto source);

        [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
        [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_PrivateValue")]
        public static extern void SetPrivateValue1(this global::Riok.Mapperly.IntegrationTests.Models.TestObject target, int value);
    }
}