using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class TestObjectDto
    {
        public TestObjectDto(int ctorValue, int unknownValue = 10, int ctorValue2 = 100)
        {
            CtorValue = ctorValue;
            CtorValue2 = ctorValue2;
        }

        public int ABCDEFGHIJKLM { get; set; }
        public int CtorValue { get; set; }

        public int CtorValue2 { get; set; }

        public int IntValue { get; set; }

        public int IntInitOnlyValue { get; init; }

#if NET7_0_OR_GREATER
        public required int RequiredValue { get; init; }
#else
        public int RequiredValue { get; init; }
#endif

        public string StringValue { get; set; } = string.Empty;

        public string RenamedStringValue2 { get; set; } = string.Empty;

        public int FlatteningIdValue { get; set; }

        public int? NullableFlatteningIdValue { get; set; }

        public IdObjectDto Unflattening { get; set; } = new();

        public IdObjectDto? NullableUnflattening { get; set; }

        public int NestedNullableIntValue { get; set; }

        public TestObjectNestedDto? NestedNullable { get; set; }

        public TestObjectNestedDto NestedNullableTargetNotNullable { get; set; } = new();

        public string StringNullableTargetNotNullable { get; set; } = string.Empty;

        public TestObjectDto? RecursiveObject { get; set; }

        public TestObject? SourceTargetSameObjectType { get; set; }

        public TestObjectNestedDto[]? NullableReadOnlyObjectCollection { get; set; }

        public Stack<int> StackValue { get; set; } = new();

        public Queue<int> QueueValue { get; set; } = new();

        public ImmutableArray<int> ImmutableArrayValue { get; set; } = ImmutableArray<int>.Empty;

        public ImmutableList<int> ImmutableListValue { get; set; } = ImmutableList<int>.Empty;

        public ImmutableHashSet<int> ImmutableHashSetValue { get; set; } = ImmutableHashSet<int>.Empty;

        public ImmutableQueue<int> ImmutableQueueValue { get; set; } = ImmutableQueue<int>.Empty;

        public ImmutableStack<int> ImmutableStackValue { get; set; } = ImmutableStack<int>.Empty;

        public ImmutableSortedSet<int> ImmutableSortedSetValue { get; set; } = ImmutableSortedSet<int>.Empty;

        public ImmutableDictionary<int, int> ImmutableDictionaryValue { get; set; } = ImmutableDictionary<int, int>.Empty;

        public ImmutableSortedDictionary<int, int> ImmutableSortedDictionaryValue { get; set; } = ImmutableSortedDictionary<int, int>.Empty;

        public TestEnumDtoByValue EnumValue { get; set; }

        public TestEnumDtoByName EnumName { get; set; }

        public byte EnumRawValue { get; set; }

        public string EnumStringValue { get; set; } = string.Empty;

        public TestEnumDtoByValue EnumReverseStringValue { get; set; }

        public InheritanceSubObjectDto? SubObject { get; set; }

        public string? IgnoredStringValue { get; set; }

        public int IgnoredIntValue { get; set; }

        public DateOnly DateTimeValueTargetDateOnly { get; set; }

        public TimeOnly DateTimeValueTargetTimeOnly { get; set; }
    }
}
