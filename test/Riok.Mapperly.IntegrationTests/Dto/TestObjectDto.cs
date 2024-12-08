using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class TestObjectDto
    {
        public TestObjectDto(int ctorValue, int unknownValue = 10, int ctorValue2 = 100)
        {
            CtorValue = ctorValue;
            CtorValue2 = ctorValue2;
            PrivateValue = ctorValue + 21;
        }

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

        public int NestedMemberId { get; set; }

        public int NestedMemberObjectIntValue { get; set; }

        public string StringNullableTargetNotNullable { get; set; } = string.Empty;

        public (int A, int)? TupleValue { get; set; }

        public TestObjectDto? RecursiveObject { get; set; }

        public TestObject? SourceTargetSameObjectType { get; set; }

        public TestObjectNestedDto[]? NullableReadOnlyObjectCollection { get; set; }

        public int[]? SpanValue { get; set; }

        public Memory<int> MemoryValue { get; set; }

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

        public ISet<int> ExistingISet { get; } = new HashSet<int>();

        public HashSet<int> ExistingHashSet { get; } = new();

        public SortedSet<int> ExistingSortedSet { get; } = new();

        public List<int> ExistingList { get; } = new();

        public ISet<int> ISet { get; set; } = new HashSet<int>();

#if NET5_0_OR_GREATER
        public IReadOnlySet<int> IReadOnlySet { get; set; } = new HashSet<int>();
#endif

        public HashSet<int> HashSet { get; set; } = [];

        public SortedSet<int> SortedSet { get; set; } = [];

        public TestEnumDtoByValue EnumValue { get; set; }

        public TestFlagsEnumDto FlagsEnumValue { get; set; }

        public TestEnumDtoByName EnumName { get; set; }

        public byte EnumRawValue { get; set; }

        public string EnumStringValue { get; set; } = string.Empty;

        public TestEnumDtoByValue EnumReverseStringValue { get; set; }

        public InheritanceSubObjectDto? SubObject { get; set; }

        public string? IgnoredStringValue { get; set; }

        public int IgnoredIntValue { get; set; }

        [Obsolete]
        public int IgnoredObsoleteValue { get; set; }

        [MapperIgnore]
        public int IgnoredMemberValue { get; set; }

        public DateOnly DateTimeValueTargetDateOnly { get; set; }

        public TimeOnly DateTimeValueTargetTimeOnly { get; set; }

        public byte[] ToByteArrayWithInstanceMethod { get; set; } = [];

        public int WithCreateMethod { get; set; }

        public byte WithCreateFromMethod { get; set; }

        public float WithFromSingleMethod { get; set; }

        public double WithCreateParamsMethod { get; set; }

        public uint WithCreateFromParamsMethod { get; set; }

        public short WithFromShortParamsMethod { get; set; }

        public decimal WithToDecimalMethod { get; set; }

        public string FormattedIntValue { get; set; } = string.Empty;

        public string FormattedDateValue { get; set; } = string.Empty;

        public int ExposePrivateValue => PrivateValue;

        private int PrivateValue { get; set; }

        public int Sum { get; set; }
    }
}
