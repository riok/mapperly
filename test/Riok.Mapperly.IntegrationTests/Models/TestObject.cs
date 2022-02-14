namespace Riok.Mapperly.IntegrationTests.Models;

public class TestObject
{
    public int IntValue { get; set; }

    public string StringValue { get; set; } = string.Empty;

    public string RenamedStringValue { get; set; } = string.Empty;

    public TestObjectNested? NestedNullable { get; set; }

    public TestObjectNested? NestedNullableTargetNotNullable { get; set; }

    public string? StringNullableTargetNotNullable { get; set; }

    public TestObject? RecursiveObject { get; set; }

    public TestObject? SourceTargetSameObjectType { get; set; }

    public IReadOnlyCollection<TestObjectNested>? NullableReadOnlyObjectCollection { get; set; }

    public TestEnum EnumValue { get; set; }

    public TestEnum EnumName { get; set; }

    public TestEnum EnumRawValue { get; set; }

    public TestEnum EnumStringValue { get; set; }

    public string EnumReverseStringValue { get; set; } = string.Empty;

    public InheritanceSubObject? SubObject { get; set; }

    public string? IgnoredStringValue { get; set; }
}
