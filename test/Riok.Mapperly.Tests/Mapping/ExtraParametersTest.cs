using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ExtraParametersTest
{
    [Fact]
    public void ExtraParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveMapMethodBody(
                """
var target = new global::B()
{
    Value = value.ToString()
};
target.StringValue = src.StringValue;
return target;
"""
            );
    }

    [Fact]
    public Task TwoExtraParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value, int id);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } public int Id { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ExtraParameterMapNested()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, C nested);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string NestedValue { get; init; } }",
            "class C { public int Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveMapMethodBody(
                """
var target = new global::B()
{
    NestedValue = nested.Value.ToString()
};
target.StringValue = src.StringValue;
return target;
"""
            );
    }

    [Fact]
    public Task ExtraParameterWithReferenceHandling()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExtraParameterWithNewInstanceMethodOnlyPassRelevantParams()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            "class A { public string StringValue { get; set; } public C Nest { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } public D Nest { get; set; } }",
            "record C(int V)",
            "record D(int V)"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExtraParameterEnumerable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B[] Map(A[] src, int value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExtraParameterUsesUserMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
partial B Map(A src, int value);
public string ToString(int value) => value.ToString();
""",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserMethodWithExtraParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
partial B Map(A src, int value);
public C MapToC(string str, int value) => new C() { StringValue = str, Value = value };
""",
            "class A { public string Nested { get; set; } }",
            "class B { public C Nested { get; init; } }",
            "class C { public string StringValue { get; init; } public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
