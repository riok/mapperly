using System;
using System.Collections.Generic;

namespace Riok.Mapperly.IntegrationTests.Models
{
    public class TestObjectProjection
    {
        public int CtorValue { get; set; }

        public int IntValue { get; set; }

        public int IntInitOnlyValue { get; init; }

#if NET7_0_OR_GREATER
        public required int RequiredValue { get; init; }
#else
        public int RequiredValue { get; init; }
#endif

        public string StringValue { get; set; } = string.Empty;

        public string RenamedStringValue { get; set; } = string.Empty;

        public IdObject Flattening { get; set; } = new();

        public IdObject? NullableFlattening { get; set; }

        public int UnflatteningIdValue { get; set; }

        public int? NullableUnflatteningIdValue { get; set; }

        public TestObjectNested? NestedNullable { get; set; }

        public TestObjectNested? NestedNullableTargetNotNullable { get; set; }

        public string? StringNullableTargetNotNullable { get; set; }

        public TestObjectProjection? RecursiveObject { get; set; }

        public TestObjectProjection? SourceTargetSameObjectType { get; set; }

        public List<TestObjectNested>? NullableReadOnlyObjectCollection { get; set; }

        public TestEnum EnumValue { get; set; }

        public TestEnum EnumName { get; set; }

        public TestEnum EnumRawValue { get; set; }

        public TestEnum EnumStringValue { get; set; }

        public string EnumReverseStringValue { get; set; } = string.Empty;

        public InheritanceSubObject? SubObject { get; set; }

        public string? IgnoredStringValue { get; set; }

        public int IgnoredIntValue { get; set; }

        public DateTime DateTimeValueTargetDateOnly { get; set; }

        public DateTime DateTimeValueTargetTimeOnly { get; set; }

        public String ManuallyMapped { get; set; } = "fooBar";
    }
}
