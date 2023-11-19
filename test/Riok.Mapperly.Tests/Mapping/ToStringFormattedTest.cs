using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ToStringFormattedTest
{
    [Fact]
    public void ClassMultiplePropertiesToStringWithDifferentFormats()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value1", "Value1", StringFormat = "dd.MM.yyyy")]
            [MapProperty("Value2", "Value2", StringFormat = "yyyy-MM-dd")]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } public DateTime Value1 { get; set; } public DateTime Value2 { get; set; } }",
            "class B { public string Value { get; set; } public string Value1 { get; set; } public string Value2 { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString();
                target.Value1 = source.Value1.ToString("dd.MM.yyyy", null);
                target.Value2 = source.Value2.ToString("yyyy-MM-dd", null);
                return target;
                """
            );
    }

    [Fact]
    public void RecordMultiplePropertiesToStringWithDifferentFormats()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value1", "Value1", StringFormat = "dd.MM.yyyy")]
            [MapProperty("Value2", "Value2", StringFormat = "yyyy-MM-dd")]
            partial B Map(A source);",
            """,
            "record A(DateTime Value, DateTime Value1, DateTime Value2);",
            "record B(string Value, string Value1, string Value2);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value.ToString(), source.Value1.ToString("dd.MM.yyyy", null), source.Value2.ToString("yyyy-MM-dd", null));
                return target;
                """
            );
    }

    [Fact]
    public void ClassToStringWithoutFormatParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "Value", StringFormat = "C")]
            partial B Map(A source);",
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "class C {}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceDoesNotImplementIFormattable,
                "The source type C does not implement IFormattable, string format cannot be applied"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString();
                return target;
                """
            );
    }
}
