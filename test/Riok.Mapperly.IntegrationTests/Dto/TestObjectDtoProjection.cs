using System;
using System.Collections.Generic;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class TestObjectDtoProjection
    {
        public TestObjectDtoProjection(int ctorValue)
        {
            CtorValue = ctorValue;
        }

        public int CtorValue { get; set; }

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

        public int NestedNullableIntValue { get; set; }

        public TestObjectNestedDto? NestedNullable { get; set; }

        public TestObjectNestedDto NestedNullableTargetNotNullable { get; set; } = new();

        public string StringNullableTargetNotNullable { get; set; } = string.Empty;

        public TestObjectProjection? SourceTargetSameObjectType { get; set; }

        public IReadOnlyCollection<TestObjectNestedDto>? NullableReadOnlyObjectCollection { get; set; }

        public TestEnumDtoByValue EnumValue { get; set; }

        public TestEnumDtoByName EnumName { get; set; }

        public byte EnumRawValue { get; set; }

        public string EnumStringValue { get; set; } = string.Empty;

        public TestEnumDtoByName EnumReverseStringValue { get; set; }

        public InheritanceSubObjectDto? SubObject { get; set; }

        public string? IgnoredStringValue { get; set; }
        public int IgnoredIntValue { get; set; }

        public DateOnly DateTimeValueTargetDateOnly { get; set; }

        public TimeOnly DateTimeValueTargetTimeOnly { get; set; }

        public TestObjectDtoManuallyMappedProjection? ManuallyMapped { get; set; }

        public int ManuallyMappedModified { get; set; }

        public List<TestEnum> ManuallyMappedList { get; set; } = new();
    }
}
